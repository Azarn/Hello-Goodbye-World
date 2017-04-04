using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Goodbye_World_Cecil {
    /**
     * This program will replace any string matching `SEARCH_STRING` with `REPLACE_STRING`
     * in any Module and Type represented in binary file at `RELATIVE_PATH`
     * and will save resulting assembly to `OUPUT_PATH`
     */
    class Program {
        public static readonly string RELATIVE_PATH = @"..\..\..\Hello World\bin\Release\Hello World.exe";
        public static readonly string OUTPUT_PATH = @"Fixed binary.exe";
        public static readonly string SEARCH_STRING = "Hello";
        public static readonly string REPLACE_STRING = "Goodbye";

        static void Main(string[] args) {
            AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(RELATIVE_PATH);
            foreach(var m in ad.Modules) {
                Console.WriteLine("Module: {0}", m);
                foreach (var t in m.Types) {
                    IterateType(m, t);
                }
            }

            ad.Write(OUTPUT_PATH);
        }

        static void IterateType(ModuleDefinition currentModule, TypeDefinition type) {
            if (type.Module != currentModule) {
                return;
            }

            Console.WriteLine("Type: {0}", type);

            foreach (var nt in type.NestedTypes) {
                IterateType(currentModule, nt);
            }

            foreach (var mi in type.Methods) {
                if (!mi.HasBody) {
                    continue;
                }

                Console.WriteLine(mi);
                foreach(var inst in mi.Body.Instructions) {
                    if (inst.OpCode == OpCodes.Ldstr) {
                        inst.Operand = ((string)inst.Operand).Replace(SEARCH_STRING, REPLACE_STRING);
                    }
                }
            }
        }
    }
}
