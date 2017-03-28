using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Goodbye_World {
    struct Instruction {
        public OpCode opCode;
        public byte[] operand;

        public override string ToString() {
            return string.Format("{0} {1}", opCode.Name, BitConverter.ToString(operand));
        }
    }

    class Program {
        public static readonly string RELATIVE_PATH = @"..\..\..\Hello World\bin\Release\Hello World.exe";
        public static readonly string SEARCH_STRING = "Hello";
        public static readonly string REPLACE_STRING = "Goodbye";
        public static readonly BindingFlags ALMOST_ALL_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic |
                                                               BindingFlags.Public | BindingFlags.Static;

        // Cecil, .!.
        public static readonly IDictionary<OperandType, int> OPERAND_SIZE_MAP = new Dictionary<OperandType, int>() {
            { OperandType.InlineBrTarget, 4 },
            { OperandType.InlineField, 4 },
            { OperandType.InlineI, 4 },
            { OperandType.InlineI8, 8 },
            { OperandType.InlineMethod, 4 },
            { OperandType.InlineNone, 0 },
            { OperandType.InlineR, 8 },
            { OperandType.InlineSig, 4 },
            { OperandType.InlineString, 4 },
            { OperandType.InlineSwitch, 4 },
            { OperandType.InlineTok, IntPtr.Size },
            { OperandType.InlineType, 4 },
            { OperandType.InlineVar, 2 },
            { OperandType.ShortInlineBrTarget, 1 },
            { OperandType.ShortInlineI, 1 },
            { OperandType.ShortInlineR, 4 },
            { OperandType.ShortInlineVar, 1 }
        };

        public static readonly IDictionary<short, OpCode> OPCODE_VALUE_MAP = new Dictionary<short, OpCode>();

        static Program() {
            // Cecil, .!. .!. .!.
            foreach (FieldInfo fi in typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly |
                                                              BindingFlags.Static | BindingFlags.Public)) {
                if (fi.FieldType == typeof(OpCode)) {
                    OpCode opcode = (OpCode)fi.GetValue(null);
                    OPCODE_VALUE_MAP.Add(opcode.Value, opcode);
                }
            }
        }

        static void Main(string[] args) {
            //Assembly a = Assembly.ReflectionOnlyLoadFrom(Path.GetFullPath(RELATIVE_PATH));
            Assembly a = Assembly.LoadFile(Path.GetFullPath(RELATIVE_PATH));
            foreach (Module m in a.GetModules()) {
                Console.WriteLine("Module: {0}", m);
                foreach (Type t in m.GetTypes()) {
                    IterateType(m, t, SEARCH_STRING, REPLACE_STRING);
                }
            }
            a.EntryPoint.Invoke(null, new object[] { args });
        }

        static List<Instruction> ParseInstuctions(byte[] src) {
            List<Instruction> res = new List<Instruction>();
            long pos = 0;
            while (pos < src.LongLength) {       // Yeah, > 2 GB IL code
                short opValue = src[pos++];
                if (!OPCODE_VALUE_MAP.ContainsKey(opValue)) {
                    opValue <<= 8;
                    opValue += src[pos++];
                }

                if (!OPCODE_VALUE_MAP.ContainsKey(opValue)) {
                    throw new Exception(string.Format("Unknown opcode: {0}", opValue));
                }

                OpCode opCode = OPCODE_VALUE_MAP[opValue];
                int operandSize = OPERAND_SIZE_MAP[opCode.OperandType];
                Instruction instr = new Instruction() {
                    opCode = opCode,
                    operand = new byte[operandSize]
                };

                for (int i = 0; i < operandSize; ++i) {
                    instr.operand[i] = src[pos++];
                }

                res.Add(instr);
                Console.WriteLine("Parsed: {0}", instr);
            }
            return res;
        }

        static void IterateType(Module currentModule, Type type, string old, string replacement) {
            if (type.Module != currentModule) {
                return;
            }

            if (type.BaseType != null) {
                IterateType(currentModule, type.BaseType, old, replacement);
            }
            Console.WriteLine("Type: {0}", type);

            foreach (Type nt in type.GetNestedTypes(ALMOST_ALL_FLAGS)) {
                IterateType(currentModule, nt, old, replacement);
            }

            foreach (MethodInfo mi in type.GetMethods(ALMOST_ALL_FLAGS | BindingFlags.DeclaredOnly)) {
                Console.WriteLine(mi);
                if (mi.GetMethodBody() == null) {
                    continue;
                }

                byte[] src = mi.GetMethodBody().GetILAsByteArray();
                Console.WriteLine("SRC Bytes:");
                Console.WriteLine(BitConverter.ToString(src));

                List<Instruction> lst = ParseInstuctions(src);
                foreach (Instruction inst in lst) {
                    if (inst.opCode == OpCodes.Ldstr) {
                        string foundString = currentModule.ResolveString(BitConverter.ToInt32(inst.operand, 0));
                        foundString = foundString.Replace(SEARCH_STRING, REPLACE_STRING);
                        Console.WriteLine(foundString);
                    }
                }
            }
        }
    }
}
