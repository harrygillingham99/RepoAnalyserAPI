using System.Collections.Generic;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RepoAnalyser.Logic.AnalysisHelpers
{
    /*
     * Helper class derived and adapted from logic in Gendarme AvoidComplexMethodsRule.cs
     */
    public static class CyclomaticComplexityHelper
    {
        private static readonly List<Instruction> Targets = new List<Instruction>();

        private static readonly OpCodeBitmask Ld = new OpCodeBitmask(0xFFFF6C3FC, 0x1B0300000000FFE0, 0x400100FFF800,
            0xDE0);

        public static int GetCyclomaticComplexity(this MethodDefinition method)
        {
            if (method == null || !method.HasBody)
                return 1;

            int GetFastCyclomaticComplexity(MethodDefinition methodDefinition)
            {
                var cc = 1;
                foreach (var ins in methodDefinition.Body.Instructions)
                    switch (ins.OpCode.FlowControl)
                    {
                        case FlowControl.Branch:
                            // detect ternary pattern
                            var previous = ins.Previous;
                            if (previous != null && Ld.Get(previous.OpCode.Code))
                                cc++;
                            break;
                        case FlowControl.Cond_Branch:
                            cc++;
                            break;
                    }

                return cc;
            }

            int GetSwitchCyclomaticComplexity(MethodDefinition methodDefinition)
            {
                Instruction previous = null;
                var cc = 1;

                foreach (var ins in methodDefinition.Body.Instructions)
                {
                    Instruction branch;
                    switch (ins.OpCode.FlowControl)
                    {
                        case FlowControl.Branch:
                            if (previous == null)
                                continue;
                            // detect ternary pattern
                            previous = ins.Previous;
                            if (Ld.Get(previous.OpCode.Code))
                                cc++;
                            // or 'default' (xmcs)
                            if (previous.OpCode.FlowControl == FlowControl.Cond_Branch)
                            {
                                branch = previous.Operand as Instruction;
                                // branch can be null (e.g. switch -> Instruction[])
                                if (branch != null && Targets.Contains(branch))
                                    Targets.AddIfNew(ins);
                            }

                            break;
                        case FlowControl.Cond_Branch:
                            // note: a single switch (C#) with sparse values can be broken into several swicth (IL)
                            // that will use the same 'targets' and must be counted only once
                            if (ins.OpCode.Code == Code.Switch)
                            {
                                AccumulateSwitchTargets(ins);
                            }
                            else
                            {
                                // some conditional branch can be related to the sparse switch
                                branch = ins.Operand as Instruction;
                                if (branch != null)
                                {
                                    previous = branch.Previous;
                                    if (previous != null && !previous.Previous.Is(Code.Switch))
                                        if (!Targets.Contains(branch))
                                            cc++;
                                }
                            }

                            break;
                    }
                }

                // count all unique targets (and default if more than one C# switch is used)
                cc += Targets.Count;
                Targets.Clear();

                return cc;
            }

            void AccumulateSwitchTargets(Instruction ins)
            {
                var cases = (Instruction[])ins.Operand;
                foreach (var target in cases)
                    // ignore targets that are the next instructions (xmcs)
                    if (target != ins.Next)
                        Targets.AddIfNew(target);

                // add 'default' branch (if one exists)
                var next = ins.Next;
                if (next.OpCode.FlowControl == FlowControl.Branch)
                {
                    var unc = FindFirstUnconditionalBranchTarget(cases[0]);
                    if (unc != next.Operand)
                        Targets.AddIfNew(next.Operand as Instruction);
                }
            }

            Instruction FindFirstUnconditionalBranchTarget(Instruction ins)
            {
                while (ins != null)
                {
                    if (FlowControl.Branch == ins.OpCode.FlowControl)
                        return (Instruction)ins.Operand;

                    ins = ins.Next;
                }

                return null;
            }

            return OpCodeEngine.GetBitmask(method).Get(Code.Switch) ? GetSwitchCyclomaticComplexity(method) : GetFastCyclomaticComplexity(method);
        }

        public static Dictionary<string, int> GetCyclomaticComplexities(this IEnumerable<MethodDefinition> methods)
        {
            var results = new Dictionary<string, int>();
            foreach (var method in methods)
            {
                results.Add(results.ContainsKey(method.Name) ? method.FullName : method.Name,
                    method.GetCyclomaticComplexity());
            }
            return results;
        }
    }
}