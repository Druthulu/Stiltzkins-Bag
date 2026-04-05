// StiltzkinsBag.Core/Parsing/FieldScriptOpcodeTable.cs
//
// Ports the HADES_STRING_SCRIPT_OPCODE table and VarOpList from
// Hades Workshop's Database_Script.cpp.
//
// Used by FieldParser.ScanFunction for sequential opcode decoding —
// the only approach that eliminates false positives from the 0x48 (AddItem)
// opcode byte appearing as argument data inside unrelated instructions.
//
// ── Opcode entry fields used for parsing ─────────────────────────────────────
//
//   UseVarArg  — if true, a vararg_flag byte precedes the arguments.
//                Each bit of vararg_flag: 0 = constant arg (read ArgLengths[i] bytes),
//                                         1 = variable arg (VarOp expression until 0x7F).
//
//                If false, no vararg_flag byte. Each arg uses ArgLengths[i] directly:
//                  ArgLengths[i] > 0  → read exactly that many bytes
//                  ArgLengths[i] == 0 → read a VarOp expression (scan to 0x7F)
//
//   ArgLengths — per-arg byte widths for constant args. Empty for 0-arg opcodes.
//
//   IsSwitch   — true for variable-length switch/jump opcodes (0x06, 0x0B, 0x0D,
//                0x29). When encountered, the sequential scanner stops the current
//                function scan. Items are still reported from functions that contain
//                these opcodes UP TO the point the switch is encountered.
//
// ── VarOp expression skipping ────────────────────────────────────────────────
//
// A VarOp expression is a sequence of tokens ending with 0x7F (terminate).
// Each token byte maps to a number of additional bytes that follow it.
//
// Key values (from VarOpList in Database_Script.cpp):
//   0x7D → 2 bytes  (const short — carries Treasure_Item values)
//   0x7E → 4 bytes  (const long)
//   0x7F → stop     (terminate marker)
//   0x80-0xBF → 0 bytes (type 100: unknown/unused operators)
//   0xC0-0xDF → 1 byte  (variable refs with 1-byte ID: VAR_GenInt16_ etc.)
//   0xE0-0xFF → 2 bytes (variable refs with 2-byte array ID: VARL_ variants)
//
// Specific type-50/51 function tokens that consume an argument byte directly
// (per VarOpTypeList in Database_Script.cpp):
//   0x4F (IsButton), 0x52 (GetHP), 0x53 (GetMaxHP),
//   0x58 (IsUnbutton), 0x59 (IsButtonDown), 0x5A (Op5A), 0x5B (Op5B), 0x5C (Op5C),
//   0x5F (ObjectUID_), 0x64 (GetItemCount), 0x65 (GetTileAnimFrame),
//   0x6A (GetAnimDuration), 0x6B (IsInParty), 0x6D (AddParty),
//   0x6E (GetMP), 0x6F (GetMaxMP), 0x70 (GetWalkpathTriangle), 0x71 (GetWalkpath),
//   0x78 (GetEntryProperty), 0x79 (SV_), 0x7A (GetData_)
//   → all consume 1 additional byte

namespace StiltzkinsBag.Core.Parsing;

/// <summary>
/// Parsed opcode descriptor for sequential field script decoding.
/// </summary>
public readonly struct OpcodeInfo
{
    /// <summary>
    /// True if a vararg_flag byte precedes the arguments.
    /// When false, all arguments use their stated ArgLengths directly
    /// (0 = VarOp expression, >0 = read exactly that many bytes).
    /// </summary>
    public readonly bool UseVarArg;

    /// <summary>
    /// Per-argument byte widths for constant-form arguments.
    /// Null or empty = this opcode has no arguments.
    /// In use_vararg=false context: 0 = VarOp expression.
    /// In use_vararg=true context: these are the constant lengths
    /// used when the corresponding vararg_flag bit is 0.
    /// </summary>
    public readonly int[] ArgLengths;

    /// <summary>
    /// True for variable-length switch/jump opcodes whose byte extent cannot
    /// be determined without runtime evaluation. The sequential scanner stops
    /// the current function scan when it encounters one of these.
    /// </summary>
    public readonly bool IsSwitch;

    public OpcodeInfo(bool useVarArg, int[] argLengths, bool isSwitch = false)
    {
        UseVarArg = useVarArg;
        ArgLengths = argLengths;
        IsSwitch = isSwitch;
    }
}

/// <summary>
/// Static lookup tables for sequential field script decoding.
/// Ported from Hades Workshop's Database_Script.cpp.
/// </summary>
public static class FieldScriptOpcodeTable
{
    // Sentinel for 0-arg opcodes — shared reference, avoid allocations
    private static readonly int[] NoArgs = Array.Empty<int>();

    // ── Opcode table ──────────────────────────────────────────────────────────
    // Key = opcode ID (0x00-0x111, excluding 0xFF which is the extension prefix)
    // Opcodes absent from this map are treated as unknown → stop function scan
    private static readonly Dictionary<int, OpcodeInfo> s_opcodes = BuildOpcodeTable();

    // ── VarOp skip table ──────────────────────────────────────────────────────
    // Index = VarOp token byte (0x00-0xFF)
    // Value = number of additional bytes to consume after reading this token
    // 0xFF (0x7F in VarOp space) is handled by the termination check, not this table
    private static readonly int[] s_varOpSkipBytes = BuildVarOpSkipTable();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up the opcode descriptor for the given opcode ID.
    /// Returns false if the opcode is unknown (stop scanning).
    /// Does not handle 0xFF (extension prefix) — caller must handle it.
    /// </summary>
    public static bool TryGet(int opcodeId, out OpcodeInfo info)
        => s_opcodes.TryGetValue(opcodeId, out info);

    /// <summary>
    /// Returns the number of additional bytes that follow this VarOp token byte.
    /// Caller must stop before calling this for 0x7F (terminate).
    /// </summary>
    public static int GetVarOpSkipBytes(byte token) => s_varOpSkipBytes[token];

    // ── Opcode table builder ──────────────────────────────────────────────────

    private static Dictionary<int, OpcodeInfo> BuildOpcodeTable()
    {
        // Helper shorthands
        static OpcodeInfo F(params int[] lens) => new(false, lens.Length == 0 ? NoArgs : lens);
        static OpcodeInfo V(params int[] lens) => new(true, lens.Length == 0 ? NoArgs : lens);
        static OpcodeInfo Sw() => new(false, NoArgs, isSwitch: true);

        // F = use_vararg false (fixed-format), V = use_vararg true (with vararg_flag)
        // Sw = switch/variable-length (stop scan on encounter)
        // Numbers are arg byte widths; 0 = VarOp expression (only valid in F entries)

        return new Dictionary<int, OpcodeInfo>
        {
            // ── 0x00-0x0F ─────────────────────────────────────────────────────
            { 0x00, F()          }, // NOTHING
            { 0x01, F(2)         }, // JMP — fixed 2-byte offset (use_vararg=false per HW)
            { 0x02, F(2)         }, // JMP_IFN
            { 0x03, F(2)         }, // JMP_IF
            { 0x04, F()          }, // return
            { 0x05, F(0)         }, // set — VarOp expression (0 = VarOp in F context)
            { 0x06, F()         }, // JMP_SWITCHEX — computed extent, handled in ScanFunction
            { 0x07, F(1, 1)      }, // InitCode
            { 0x08, F(1, 1)      }, // InitRegion
            { 0x09, F(1, 1)      }, // InitObject
            { 0x0A, F()          }, // unused
            { 0x0B, F()         }, // JMP_SWITCH — computed extent, handled in ScanFunction
            { 0x0C, F()          }, // unknown
            { 0x0D, Sw()         }, // unknown (JMP_SWITCH with short case count), stop
            { 0x0E, F()          }, // unused
            { 0x0F, F()          }, // unused

            // ── 0x10-0x1F ─────────────────────────────────────────────────────
            { 0x10, V(1, 1, 1)   }, // RunScriptAsync
            { 0x11, F()          }, // unused
            { 0x12, V(1, 1, 1)   }, // RunScript
            { 0x13, F()          }, // unused
            { 0x14, V(1, 1, 1)   }, // RunScriptSync
            { 0x15, F()          }, // unused
            { 0x16, V(1, 1)      }, // RunScriptObjectAsync
            { 0x17, F()          }, // unused
            { 0x18, V(1, 1)      }, // RunScriptObject
            { 0x19, F()          }, // unused
            { 0x1A, V(1, 1)      }, // RunScriptObjectSync
            { 0x1B, V(1)         }, // ContinueBattleMusic
            { 0x1C, V(1)         }, // TerminateEntry
            { 0x1D, V(2, 2)      }, // CreateObject
            { 0x1E, V(1, 2, 2, 2, 2) }, // SetCameraBounds
            { 0x1F, V(1, 1, 2)   }, // WindowSync

            // ── 0x20-0x2F ─────────────────────────────────────────────────────
            { 0x20, V(1, 1, 2)   }, // WindowAsync
            { 0x21, V(1)         }, // CloseWindow
            { 0x22, V(1)         }, // Wait
            { 0x23, V(2, 2)      }, // Walk
            { 0x24, V(1)         }, // WalkTowardObject
            { 0x25, F()          }, // InitWalk
            { 0x26, V(1)         }, // SetWalkSpeed
            { 0x27, V(1)         }, // SetTriangleFlagMask
            { 0x28, V(1, 1, 1, 1)}, // Cinematic
            { 0x29, Sw()         }, // SetRegion — variable polygon vertex count, stop
            { 0x2A, V(1, 2)      }, // Battle
            { 0x2B, V(2)         }, // Field
            { 0x2C, F()          }, // DefinePlayerCharacter
            { 0x2D, F()          }, // DisableMove
            { 0x2E, F()          }, // EnableMove
            { 0x2F, V(2, 1)      }, // SetModel

            // ── 0x30-0x3F ─────────────────────────────────────────────────────
            { 0x30, F()          }, // unknown (PRINT1)
            { 0x31, F()          }, // unknown (PRINTF)
            { 0x32, V(1, 1)      }, // unused (LOCATE)
            { 0x33, V(2)         }, // SetStandAnimation
            { 0x34, V(2)         }, // SetWalkAnimation
            { 0x35, V(2)         }, // SetRunAnimation
            { 0x36, V(1)         }, // TurnInstant
            { 0x37, V(1, 1)      }, // SetPitchAngle
            { 0x38, V(1)         }, // Attack
            { 0x39, V(1, 1)      }, // ShowObject
            { 0x3A, V(1, 1)      }, // HideObject
            { 0x3B, V(1)         }, // SetObjectIndex
            { 0x3C, V(1, 2, 2, 2, 2) }, // SetRandomBattles
            { 0x3D, V(1, 1)      }, // SetAnimationInOut
            { 0x3E, V(1)         }, // SetAnimationSpeed
            { 0x3F, V(1, 1)      }, // SetAnimationFlags

            // ── 0x40-0x4F ─────────────────────────────────────────────────────
            { 0x40, V(2)         }, // RunAnimation
            { 0x41, F()          }, // WaitAnimation
            { 0x42, F()          }, // StopAnimation
            { 0x43, V(1)         }, // RunSharedScript
            { 0x44, F()          }, // WaitSharedScript
            { 0x45, F()          }, // StopSharedScript
            { 0x46, F()          }, // 0x46 (DEBUGCC)
            { 0x47, V(1)         }, // EnableHeadFocus
            { 0x48, V(2, 1)      }, // AddItem ★ item=uint16, count=uint8
            { 0x49, V(2, 1)      }, // RemoveItem
            { 0x4A, V(1, 2)      }, // RunBattleCode
            { 0x4B, V(1, 1, 1)   }, // SetObjectLogicalSize
            { 0x4C, V(1, 1, 1)   }, // AttachObject
            { 0x4D, V(1)         }, // DetachObject
            { 0x4E, F()          }, // 0x4E (WATCH)
            { 0x4F, V(1)         }, // 0x4F (STOP)

            // ── 0x50-0x5F ─────────────────────────────────────────────────────
            { 0x50, F()          }, // WaitTurn
            { 0x51, V(1, 1)      }, // TurnTowardObject
            { 0x52, V(2)         }, // SetInactiveAnimation
            { 0x53, F()          }, // PreventWindowInit
            { 0x54, V(1)         }, // WaitWindow
            { 0x55, V(1)         }, // SetWalkTurnSpeed
            { 0x56, V(1, 1)      }, // TimedTurn
            { 0x57, V(1)         }, // SetRandomBattleFrequency
            { 0x58, V(2, 2, 2)   }, // SlideXZY
            { 0x59, V(1, 1, 1, 1)}, // SetTileColor
            { 0x5A, V(1, 2, 2, 2)}, // SetTilePositionEx
            { 0x5B, V(1, 1)      }, // ShowTile
            { 0x5C, V(1, 1, 2, 2)}, // MoveTileLoop
            { 0x5D, V(1, 1, 2, 2)}, // MoveTile
            { 0x5E, V(1, 2, 2)   }, // SetTilePosition
            { 0x5F, V(1, 1)      }, // RunTileAnimation

            // ── 0x60-0x6F ─────────────────────────────────────────────────────
            { 0x60, V(1, 1)      }, // ActivateTileAnimation
            { 0x61, V(1, 2)      }, // SetTileAnimationSpeed
            { 0x62, V(1, 1)      }, // SetRow
            { 0x63, V(1, 1, 1)   }, // SetTileAnimationPause
            { 0x64, V(1, 1)      }, // SetTileAnimationFlags
            { 0x65, V(1, 1, 1)   }, // RunTileAnimationEx
            { 0x66, V(1, 2)      }, // SetTextVariable ★ slot=uint8, value=uint16
            { 0x67, V(1, 1)      }, // SetControlDirection
            { 0x68, V(1)         }, // Bubble
            { 0x69, V(2)         }, // ChangeTimerTime
            { 0x6A, F()          }, // DisableRun
            { 0x6B, V(1, 1, 1)   }, // SetBackgroundColor
            { 0x6C, F()          }, // 0x6C (PPRINT)
            { 0x6D, F()          }, // 0x6D (PPRINTF)
            { 0x6E, F()          }, // 0x6E (MAPID)
            { 0x6F, V(2, 2, 1, 1)}, // MoveCamera

            // ── 0x70-0x7F ─────────────────────────────────────────────────────
            { 0x70, V(1, 1)      }, // ReleaseCamera
            { 0x71, V(1, 1, 1)   }, // EnableCameraServices
            { 0x72, V(2)         }, // SetCameraFollowHeight
            { 0x73, F()          }, // EnableCameraFollow
            { 0x74, F()          }, // DisableCameraFollow
            { 0x75, V(1, 1)      }, // Menu
            { 0x76, V(2, 2)      }, // DrawRegionStart
            { 0x77, V(2, 2)      }, // DrawRegionSetLast
            { 0x78, F()          }, // DrawRegionPushNew
            { 0x79, F()          }, // 0x79 (PRINTQUAD)
            { 0x7A, V(2)         }, // SetLeftAnimation
            { 0x7B, V(2)         }, // SetRightAnimation
            { 0x7C, V(2, 1)      }, // EnableDialogChoices
            { 0x7D, V(1)         }, // RunTimer
            { 0x7E, V(1)         }, // SetFieldCamera
            { 0x7F, F()          }, // EnableShadow

            // ── 0x80-0x8F ─────────────────────────────────────────────────────
            { 0x80, F()          }, // DisableShadow
            { 0x81, V(1, 1)      }, // SetShadowSize
            { 0x82, V(2, 2)      }, // SetShadowOffset
            { 0x83, V(1)         }, // LockShadowRotation
            { 0x84, F()          }, // UnlockShadowRotation
            { 0x85, V(1)         }, // SetShadowAmplifier
            { 0x86, V(1, 1, 1, 1)}, // SetAnimationStandSpeed
            { 0x87, V(1, 1)      }, // TurnInstantEx
            { 0x88, V(1, 2, 2, 2)}, // RunModelCode
            { 0x89, V(2, 2, 2, 1)}, // SetSoundPosition
            { 0x8A, V(1, 1)      }, // SetSoundObjectPosition
            { 0x8B, V(1, 1)      }, // SetHeadFocusMask
            { 0x8C, V(1, 1, 2)   }, // BattleEx
            { 0x8D, V(1)         }, // ShowTimer
            { 0x8E, F()          }, // RaiseWindows
            { 0x8F, V(1, 1, 1, 1)}, // SetModelColor

            // ── 0x90-0x9F ─────────────────────────────────────────────────────
            { 0x90, F()          }, // DisableInactiveAnimation
            { 0x91, V(1)         }, // FollowFocus
            { 0x92, V(1, 2, 1, 1, 1, 1, 1) }, // AttachTile
            { 0x93, V(1)         }, // SetObjectFlags
            { 0x94, V(2, 1, 1)   }, // SetJumpAnimation
            { 0x95, V(1, 1, 1, 2)}, // WindowSyncEx
            { 0x96, V(1, 1, 1, 2)}, // WindowAsyncEx
            { 0x97, V(1)         }, // ReturnEntryFunctions
            { 0x98, V(1)         }, // MakeAnimationLoop
            { 0x99, V(1)         }, // SetTurnSpeed
            { 0x9A, V(2, 1)      }, // EnablePathTriangle
            { 0x9B, V(2, 2)      }, // TurnTowardPosition
            { 0x9C, F()          }, // RunJumpAnimation
            { 0x9D, F()          }, // RunLandAnimation
            { 0x9E, F()          }, // ExitField
            { 0x9F, V(1, 1, 1, 1)}, // SetObjectSize

            // ── 0xA0-0xAF ─────────────────────────────────────────────────────
            { 0xA0, F()          }, // WalkToExit
            { 0xA1, V(2, 2, 2)   }, // MoveInstantXZY
            { 0xA2, V(2, 2, 2)   }, // WalkXZY
            { 0xA3, V(1, 1, 1, 1)}, // 0xA3 (DRADIUS)
            { 0xA4, F()          }, // CalculateExitPosition
            { 0xA5, V(2, 2)      }, // Slide
            { 0xA6, V(1)         }, // SetRunSpeedLimit
            { 0xA7, V(1)         }, // Turn
            { 0xA8, V(1)         }, // SetPathing
            { 0xA9, V(1)         }, // CalculateScreenPosition
            { 0xAA, F()          }, // EnableMenu
            { 0xAB, F()          }, // DisableMenu
            { 0xAC, V(2)         }, // ChangeDisc
            { 0xAD, V(1, 2, 2, 2)}, // MoveInstantXZYEx
            { 0xAE, V(2)         }, // TetraMaster
            { 0xAF, F()          }, // DeleteAllCards

            // ── 0xB0-0xBF ─────────────────────────────────────────────────────
            { 0xB0, V(2)         }, // SetFieldName
            { 0xB1, F()          }, // ResetFieldName
            { 0xB2, V(1, 2)      }, // Party
            { 0xB3, V(1, 1, 2, 2, 2) }, // RunSPSCode
            { 0xB4, V(2)         }, // SetPartyReserve
            { 0xB5, V(1)         }, // PretendToBe
            { 0xB6, V(2)         }, // WorldMap
            { 0xB7, F()          }, // 0xB7 (EYE world map)
            { 0xB8, F()          }, // 0xB8 (AIM world map)
            { 0xB9, V(1, 2)      }, // AddControllerMask
            { 0xBA, V(1, 2)      }, // RemoveControllerMask
            { 0xBB, V(1, 1, 1)   }, // TimedTurnEx
            { 0xBC, V(1)         }, // WaitTurnEx
            { 0xBD, V(1, 2)      }, // RunAnimationEx
            { 0xBE, V(1)         }, // WaitAnimationEx
            { 0xBF, V(1, 2, 2)   }, // MoveInstantEx

            // ── 0xC0-0xCF ─────────────────────────────────────────────────────
            { 0xC0, V(1, 1)      }, // EnableTextureAnimation
            { 0xC1, V(1, 1)      }, // RunTextureAnimation
            { 0xC2, V(1, 1)      }, // StopTextureAnimation
            { 0xC3, V(1, 1)      }, // SetTileCamera
            { 0xC4, V(1, 2)      }, // RunWorldCode
            { 0xC5, V(2, 2)      }, // RunSoundCode
            { 0xC6, V(2, 2, 3)   }, // RunSoundCode1
            { 0xC7, V(2, 2, 3, 1)}, // RunSoundCode2
            { 0xC8, V(2, 2, 3, 1, 1) }, // RunSoundCode3
            { 0xC9, V(1, 2, 2, 2, 2) }, // SetupTileLoopingWindow
            { 0xCA, V(1, 1)      }, // ResetTileAnimation
            { 0xCB, V(1, 1)      }, // EnablePath
            { 0xCC, V(2)         }, // AddCharacterAttribute
            { 0xCD, V(2)         }, // RemoveCharacterAttribute
            { 0xCE, V(3)         }, // AddGil ★ amount=int24 (AT_SPIN=3 bytes)
            { 0xCF, V(3)         }, // RemoveGil

            // ── 0xD0-0xDF ─────────────────────────────────────────────────────
            { 0xD0, V(2)         }, // BattleDialog
            { 0xD1, F()          }, // unknown (GLOBALCLEAR)
            { 0xD2, F()          }, // unknown (DEBUGSAVE)
            { 0xD3, F()          }, // unknown (DEBUGLOAD)
            { 0xD4, V(2, 2, 2)   }, // AttachObjectOffset
            { 0xD5, F()          }, // HideAllObjects
            { 0xD6, F()          }, // ShowAllObjects
            { 0xD7, V(1)         }, // ATE
            { 0xD8, V(1, 1)      }, // SetWeather
            { 0xD9, V(1, 1)      }, // CureStatus
            { 0xDA, V(1, 1, 1, 2, 2) }, // RunSPSCodeSimple
            { 0xDB, V(1, 1)      }, // EnableVictoryPose
            { 0xDC, F()          }, // Jump
            { 0xDD, V(1)         }, // RemoveParty
            { 0xDE, V(1, 2)      }, // SetName
            { 0xDF, V(1)         }, // SetObjectOvalRatio

            // ── 0xE0-0xEF ─────────────────────────────────────────────────────
            { 0xE0, F()          }, // AddFrog
            { 0xE1, F()          }, // TerminateBattle
            { 0xE2, V(2, 2, 2, 1)}, // SetupJump
            { 0xE3, V(1)         }, // SetDialogProgression
            { 0xE4, V(1, 1, 2, 2, 1) }, // MoveTileLoopWithOffset
            { 0xE5, V(1)         }, // AttackSpecial
            { 0xE6, V(1, 1)      }, // SetTileLoopType
            { 0xE7, V(1, 1)      }, // SetTileAnimationFrame
            { 0xE8, V(2, 2, 2)   }, // SideWalkXZY
            { 0xE9, F()          }, // UpdatePartyUID
            { 0xEA, F()          }, // CalculateScreenOrigin
            { 0xEB, F()          }, // CloseAllWindows
            { 0xEC, V(1, 1, 1, 1, 1, 1) }, // FadeFilter
            { 0xED, V(1, 2, 2)   }, // SetTileLoopAlpha
            { 0xEE, F()          }, // EnableInactiveAnimation
            { 0xEF, V(1)         }, // ShowHereIcon

            // ── 0xF0-0xFF ─────────────────────────────────────────────────────
            { 0xF0, F()          }, // EnableRun
            { 0xF1, V(1, 2)      }, // SetHP
            { 0xF2, V(1, 2)      }, // SetMP
            { 0xF3, V(1, 1)      }, // UnlearnAbility
            { 0xF4, V(1, 1)      }, // LearnAbility
            { 0xF5, F()          }, // GameOver
            { 0xF6, V(1)         }, // VibrateController
            { 0xF7, V(1)         }, // ActivateVibration
            { 0xF8, V(1, 1, 1)   }, // RunVibrationTrack
            { 0xF9, V(1, 1, 1)   }, // ActivateVibrationTrack
            { 0xFA, V(2)         }, // SetVibrationSpeed
            { 0xFB, V(1)         }, // SetVibrationFlags
            { 0xFC, V(1, 1)      }, // SetVibrationRange
            { 0xFD, V(1, 2)      }, // PreloadField
            { 0xFE, V(1, 1, 1, 1, 1) }, // SetCharacterData
            // 0xFF = EXTENDED_CODE prefix (not a standalone opcode — caller reads next byte)

            // ── Extended opcodes (0x100+, preceded by 0xFF in bytecode) ───────
            { 0x100, V(1, 1)     }, // 0x100 BSSTART
            { 0x101, V(1, 1)     }, // 0x101 BSFRAME
            { 0x102, V(1, 1)     }, // 0x102 BSACTIVE
            { 0x103, V(1, 1)     }, // 0x103 BSFLAG
            { 0x104, V(1, 1)     }, // 0x104 BSFLOOR
            { 0x105, V(1, 2)     }, // 0x105 BSRATE
            { 0x106, V(1, 1)     }, // 0x106 BSALGO
            { 0x107, V(1, 2, 2)  }, // 0x107 BSDELTA
            { 0x108, V(1, 1)     }, // 0x108 BSAXIS
            { 0x109, V(1, 1)     }, // 0x109 BAANIME
            { 0x10A, V(1, 1)     }, // SetWalkpathAnimFrame
            { 0x10B, V(1, 1)     }, // 0x10B BAACTIVE
            { 0x10C, V(1, 1)     }, // 0x10C BAFLAG
            { 0x10D, V(1, 2)     }, // 0x10D BARATE
            { 0x10E, V(1, 1)     }, // 0x10E BAWAITALL
            { 0x10F, V(1, 1, 1)  }, // 0x10F BAWAIT
            { 0x110, V(1, 1, 1)  }, // 0x110 BARANGE
            { 0x111, V(1, 1)     }, // 0x111 BAVISIBLE
        };
    }

    // ── VarOp skip table builder ──────────────────────────────────────────────

    private static int[] BuildVarOpSkipTable()
    {
        // Default: 0 extra bytes for all tokens (operators, unknowns)
        var table = new int[256];

        // ── Constant value tokens ─────────────────────────────────────────────
        // 0x7D: const short (type 6) → 2 bytes of value data follow
        table[0x7D] = 2;
        // 0x7E: const long (type 7) → 4 bytes of value data follow
        table[0x7E] = 4;
        // 0x7F: terminate (type -1) → caller must check this before calling here

        // ── 1-byte array variable references (0xC0-0xDF) ─────────────────────
        // VAR_Gen/Glob/LocSBool_, VAR_Gen/Glob/LocBool_ (type 13): 1 byte var ID
        // VAR_Gen/Glob/LocInt24_, VAR_Gen/Glob/LocUInt24_ (type 12): 1 byte var ID
        // VAR_Gen/Glob/LocInt8_, VAR_Gen/Glob/LocUInt8_ (type 10): 1 byte var ID
        // VAR_Gen/Glob/LocInt16_, VAR_Gen/Glob/LocUInt16_ (type 11): 1 byte var ID
        // VAR_Null* (type 19): 1 byte var ID
        // MemoriaVarCode_ (type 60, 0xD3): 1 byte custom function ID
        for (int i = 0xC0; i <= 0xDF; i++) table[i] = 1;

        // ── 2-byte array variable references (0xE0-0xFF) ─────────────────────
        // VARL_ variants (types 20, 21, 22, 23, 29): 2-byte array index
        for (int i = 0xE0; i <= 0xFF; i++) table[i] = 2;

        // ── SV_ and GetData_ (0x79, 0x7A) ────────────────────────────────────
        // type 19 (null array, 1-byte): variable ID follows
        table[0x79] = 1; // SV_ (VARCODE_SHARED)
        table[0x7A] = 1; // GetData_ (VARCODE_ENGINE)

        // ── GetEntryProperty (0x78, type 55) ─────────────────────────────────
        table[0x78] = 1; // property type byte follows

        // ── Type-50 functions with a 1-byte direct argument ───────────────────
        // Per VarOpTypeList in Database_Script.cpp:
        table[0x4F] = 1; // IsButton (AT_BUTTONLIST)
        table[0x52] = 1; // GetHP (AT_LCHARACTER)
        table[0x53] = 1; // GetMaxHP (AT_LCHARACTER)
        table[0x58] = 1; // IsUnbutton (AT_BUTTONLIST)
        table[0x59] = 1; // IsButtonDown (AT_BUTTONLIST)
        table[0x5A] = 1; // Op5A — button check, consistent with IsButton group
        table[0x5B] = 1; // Op5B — button check
        table[0x5C] = 1; // Op5C — button check
        table[0x5F] = 1; // ObjectUID_ (type 19, 1-byte array)
        table[0x64] = 0; // GetItemCount — argument is stack-based (pushed before token)
        table[0x65] = 1; // GetTileAnimFrame (AT_TILEANIM)
        table[0x6A] = 1; // GetAnimDuration (AT_ANIMATION)
        table[0x6B] = 1; // IsInParty (AT_LCHARACTER)
        table[0x6D] = 1; // AddParty (AT_LCHARACTER)
        table[0x6E] = 1; // GetMP (AT_LCHARACTER)
        table[0x6F] = 1; // GetMaxMP (AT_LCHARACTER)
        table[0x70] = 1; // GetWalkpathTriangle (AT_ENTRY)
        table[0x71] = 1; // GetWalkpath (AT_ENTRY)

        // All other tokens in 0x00-0x7C, 0x80-0xBF: 0 extra bytes (operators,
        // unary/binary ops, casts, stack-based functions with no direct arg bytes)

        return table;
    }
}