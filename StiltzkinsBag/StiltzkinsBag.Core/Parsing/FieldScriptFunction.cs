using System;
using System.Collections.Generic;

namespace StiltzkinsBag.Core.Parsing
{
    /// <summary>
    /// Port of HW's ScriptFunction.
    /// Represents one script function (entry-point) within a field script entry.
    ///
    /// IMPORTANT: <see cref="Length"/> must be set by the caller (FieldScript) from the
    /// function_point delta table BEFORE calling <see cref="Read"/>.  This mirrors HW's
    ///   func[j].length = point[j+1] - point[j];
    ///   (last function) func[j].length = entry_size[i] - point[last] - 2;
    /// </summary>
    public sealed class FieldScriptFunction
    {
        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>
        /// Byte length of this function's opcode stream, as read from the entry's
        /// function_point offset table.  Must be set before calling Read().
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Decoded operations in order.
        /// </summary>
        public List<FieldScriptOperation> Operations { get; } = new();

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Decode operations from <paramref name="data"/> starting at <paramref name="pos"/>,
        /// consuming exactly <see cref="Length"/> bytes.
        /// </summary>
        /// <param name="data">Full script binary blob.</param>
        /// <param name="pos">Current read position; advanced by exactly Length on return.</param>
        /// <exception cref="InvalidOperationException">
        ///   Thrown if Length has not been set (is 0) or if the consumed bytes do not match Length.
        /// </exception>
        public void Read(byte[] data, ref int pos)
        {
            if (Length < 0)
                throw new InvalidOperationException(
                    "FieldScriptFunction.Length must be set before calling Read().");

            // Length == 0 is valid: an empty function body (no operations).
            // HW's while (len < length) simply never executes in this case.
            if (Length == 0) return;

            Operations.Clear();
            int start = pos;
            int end   = pos + Length;

            while (pos < end)
            {
                var op = FieldScriptOperation.Read(data, ref pos);
                Operations.Add(op);
            }

            int consumed = pos - start;
            if (consumed != Length)
                throw new InvalidOperationException(
                    $"FieldScriptFunction consumed {consumed} bytes but Length was {Length}. " +
                    $"Possible decode error near offset 0x{pos:X}.");
        }

        // ── Write ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Serialize all operations into <paramref name="output"/> in order.
        /// </summary>
        public void Write(List<byte> output)
        {
            foreach (var op in Operations)
                op.Write(output);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Sum of <see cref="FieldScriptOperation.ByteSize"/> for all operations.
        /// Used by FieldScript.Write to build the function_point offset table before
        /// writing the binary header.
        /// </summary>
        public int ComputeEncodedSize()
        {
            int total = 0;
            foreach (var op in Operations)
                total += op.ByteSize;
            return total;
        }
    }
}
