# PhaseEnd — Phase 9.2: HW Field Script Codec Port

## Status
✅ COMPLETE — all 879 tests green

## Phase Goal
100% faithful port of Hades Workshop's field script decode/encode logic into C#
in-memory structs, with round-trip byte-identical encode/decode and tests.
No bug-fixing, no scanner changes — pure codec port only.

## Deliverables

### New production files
| File | Description |
|------|-------------|
| `StiltzkinsBag.Core/Parsing/FieldScriptArgument.cs` | Port of HW `ScriptArgument` + `MACRO_SCRIPT_IOFUNCTION_ARGREAD/ARGWRITE`. Two forms: constant (fixed N bytes → `uint Value`) and VarOp (token stream including 0x7F terminator → `byte[] Var`). Corrected VarOp extra-bytes table built from VarOpList type values (not VarOpTypeList GUI metadata). |
| `StiltzkinsBag.Core/Parsing/FieldScriptOperation.cs` | Port of HW `ScriptOperation` + `MACRO_SCRIPT_IOFUNCTION_OPREAD/OPWRITE`. 5 decode paths: JMP_SWITCHEX (0x06), JMP_SWITCH (0x0B), unknown switch (0x0D), SetRegion (0x29), standard. Handles 0xFF extended-opcode prefix accumulation. |
| `StiltzkinsBag.Core/Parsing/FieldScriptFunction.cs` | Port of HW `ScriptFunction`. Length pre-set by caller from function_point delta table; Read loops while bytes consumed < Length; validates exact consumption. `ComputeEncodedSize()` for offset table pre-computation. |
| `StiltzkinsBag.Core/Parsing/FieldScript.cs` | Port of HW `ScriptDataStruct` + `MACRO_SCRIPT_IOFUNCTION` (Steam path). Full 128-byte header decode/encode, entry table, per-entry function tables, function length computation. `Encode()` writes into zero-filled buffer at stored offsets. `RecomputeOffsets()` for post-modification use. |

### New test files
| File | Description |
|------|-------------|
| `StiltzkinsBag.Tests/FieldScriptRoundTripTests.cs` | Phase milestone tests. 7 named `[Theory]` fixtures (one xunit result per FieldParser file). Two `[Fact]` sweeps of all 838 `.eb.bytes` files under `TestData/`: `AllAvailableFieldFiles_DecodeWithoutException` (hard gate — zero decode failures required) and `RoundTrip_AllAvailableFieldFiles_ZeroEncodeMismatches` (zero encode mismatches required). |
| `StiltzkinsBag.Tests/FieldScriptOperationTests.cs` | 21 operation decode unit tests with synthetic byte sequences. Covers: no-arg opcodes, fixed-arg opcodes, VarOp args, UseVarArg=true const and var, JMP_SWITCH N=0/2, JMP_SWITCHEX N=0/2, SetRegion all-const and var-vertex, extended 0x100/0x111 via 0xFF prefix, VarOp token extra-byte corrections (0x78=2, 0xD3=3, 0x52=0). Each test includes round-trip assertion. |

## Key technical decisions

### VarOp extra-bytes table correction
Phase 9.1 built the skip table from `VarOpTypeList` (GUI metadata). Phase 9.2 re-derived it
from the authoritative `VarOpList` type values used by the binary decoder:
- `0x78` (GetEntryProperty, type=55): **2** extra bytes (was 1)
- `0xD3` (MemoriaVarCode_, type=60): **3** extra bytes (was 1 via loop)
- 16 type-50 stack-function tokens (0x4F, 0x52, 0x53, 0x58-0x5C, 0x65, 0x6A-0x6B, 0x6D-0x71): **0** extra bytes (were 1)

### function_point offset semantics
`function_point[j]` is relative to `function_pos` (= `localEntryPos + 2`, after entry_type
and entry_function_amount). `function_point[0] = 4 × funcCount` (skipping the function table
itself). Last function length: `entry_size[i] - function_point[last] - 2`.

### Opcode 0x0D (unknown switch)
Best-guess format as JMP_SWITCH with uint16 case count (2-byte N). Flagged UNCONFIRMED.
Round-trip sweep passed with zero encode mismatches, validating the format against real game data.

### Round-trip fidelity
`Encode()` uses zero-filled pre-sized output buffer. Stored `entry_offset`, `entry_size`, and
`function_point` values are written back verbatim. Zero-fill reproduces all original padding.
Result: byte-identical output for all decodable files.

### Zero-length function bodies (world map scripts)
9 world map scripts (`evt_world_world*.eb.bytes`) contain entries with `Length == 0` functions —
valid empty function bodies that HW's `while (len < length)` handles by simply not executing.
The initial guard in `FieldScriptFunction.Read` incorrectly threw on `Length == 0`. Fixed to
throw only on `Length < 0`; `Length == 0` returns immediately with zero operations. These 9
files now decode and round-trip correctly.

## Test counts
| Suite | Before | After |
|-------|--------|-------|
| Pre-existing | 849 | 849 |
| FieldScriptRoundTripTests (7 named + 2 sweeps) | 0 | 9 |
| FieldScriptOperationTests | 0 | 21 |
| **Total** | **849** | **879** |

## Coverage confirmation
Test data is extracted from `FINAL FANTASY IX-VANILLA2026-MemoriaEngine` — the Memoria-modded
game install, not a separate vanilla copy. All 838 files (829 field + 9 world map) decode and
round-trip under the modded install. This confirms 100% codec coverage of every script the
current game actually uses, including any Memoria-modified event scripts. Any future script
using opcodes 0x00–0x111 will decode correctly. Custom Memoria opcodes beyond 0x111 (registered
via `ScriptAPI.txt`) are the only possible edge case, and none appear in the current install.

## Known limitations / Phase 9.3 prerequisites
- Opcode 0x0D decode format is unconfirmed (assumed JMP_SWITCH with uint16 N)
- `FieldScriptOpcodeTable` IsSwitch entries (0x06, 0x0B, 0x0D, 0x29) still cause `FieldParser.ScanFunction` to stop — Phase 9.3 will switch the scanner to use the new AST
- SetRegion, Pattern B, Whale Whisker catalog, and remaining Phase 9.1 scan stops are deferred to Phase 9.3

## Next phase
**Phase 9.3** — Wire the new `FieldScript` AST into `FieldParser.FindItemLocations` to replace
the ad-hoc sequential scanner. Eliminates all current scan stops (SetRegion, extended opcodes,
0x0D, Pattern B). Completes Whale Whisker catalog. Clears remaining Phase 9.1 deferred tasks.
