﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using System.Threading.Tasks;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning;

namespace ProseTutorial
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar) { }

        // We will use this set of regular expressions in this tutorial 
        public static Regex[] UsefulRegexes = {
    new Regex(@"\w+"),  // Word
	new Regex(@"\d+"),  // Number
    new Regex(@"\s+"),  // Space
    new Regex(@".+"),  // Anything
    new Regex(@"$")  // End of line
};


        [WitnessFunction(nameof(Semantics.Substring), 1)]
        public DisjunctiveExamplesSpec WitnessStartPosition(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.Examples) {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var output = example.Value as string;
                var occurrences = new List<int>();

                for (int i = input.IndexOf(output); i >= 0; i = input.IndexOf(output, i + 1)) {
                    occurrences.Add((int)i);
                }

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);

        }

        [WitnessFunction(nameof(Semantics.Substring), 2)]
        public DisjunctiveExamplesSpec WitnessEndPosition(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.Examples) {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var output = example.Value as string;
                var occurrences = new List<int>();
                for (int i = input.IndexOf(output); i >= 0; i = input.IndexOf(output, i + 1)) {
                    occurrences.Add(i + output.Length);
                }
                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        /// <summary>
        /// This witness function should deduce the spec for k given the spect for AbsPos     
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="spec"></param>
        /// <returns>However, now we need to produce two possible specs for k (positive and negative)
        /// given a single spec for AbsPos. A disjunction of possible specs has its own 
        /// representative spec type in PROSE – DisjunctiveExamplesSpec.</returns>
        [WitnessFunction(nameof(Semantics.AbsPos), 1)]
        public DisjunctiveExamplesSpec WitnessK(GrammarRule rule, DisjunctiveExamplesSpec spec) {

            //the spec on k for each input state will have type IEnumerable<object> since we will have 
            //more than one possible output
            var kExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples) {
                State inputState = example.Key;
                var v = inputState[rule.Body[0]] as string;

                var positions = new List<int>();
                foreach (int pos in example.Value) {
                    //the positive spec for k
                    positions.Add((int)pos + 1);
                    positions.Add((int)pos - v.Length - 1);
                }
                if (positions.Count == 0) return null;
                kExamples[inputState] = positions.Cast<object>();
            }
            return DisjunctiveExamplesSpec.From(kExamples);
        }

        /// <summary>
        /// This witness function deduces a spec on rr given the spec on its operator, RelPos
        /// To do so, we need to learn a list of regular expressions that match to the left and to the right of given position. 
        /// There are many techniques for doing that; in this tutorial, we assume that we have a predefined list of 
        /// “common” regexes like /[0-9]+/, and enumerate them exhaustively at a given position.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="spec">The spec on RelPos, which is a position in the input string</param>
        /// <returns></returns>
        [WitnessFunction(nameof(Semantics.RelPos), 1)]
        public DisjunctiveExamplesSpec WitnessRegexPair(GrammarRule rule, DisjunctiveExamplesSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples) {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;

                var regexes = new List<Tuple<Regex, Regex>>();
                foreach (int output in example.Value) {
                    //TODO, complete the witness function for the rr parameter. 
                    //Given the position in the output variable above, you need to generate 
                    //all pairs of regular expressions that match this position. 
                    //you can use the auxiliar function bellow to get the regular expressions 
                    //that match each position in the input strng
                    //List<Regex>[] leftMatches, rightMatches;
                    //BuildStringMatches(input, out leftMatches, out rightMatches);


                    //var leftRegex = leftMatches[output];
                    //var rightRegex = rightMatches[output];
                    //if (leftRegex.Count == 0 || rightRegex.Count == 0)
                    //    return null;
                    //regexes.AddRange(from l in leftRegex
                    //                 from r in rightRegex
                    //                 select Tuple.Create(l, r));
                }
                if (regexes.Count == 0) return null; 
                result[inputState] = regexes;
            }
            return DisjunctiveExamplesSpec.From(result);
        }

        /// <summary>
        /// This method returns the left and the right regular expressions that match each position in the input string
        /// </summary>
        /// <param name="inp"></param>
        /// <param name="leftMatches"></param>
        /// <param name="rightMatches"></param>
        static void BuildStringMatches(string inp, out List<Regex>[] leftMatches,
                                       out List<Regex>[] rightMatches) {
            leftMatches = new List<Regex>[inp.Length + 1];
            rightMatches = new List<Regex>[inp.Length + 1];
            for (int p = 0; p <= inp.Length; ++p) {
                leftMatches[p] = new List<Regex>();
                rightMatches[p] = new List<Regex>();
            }
            foreach (Regex r in UsefulRegexes) {
                foreach (Match m in r.Matches(inp)) {
                    leftMatches[m.Index + m.Length].Add(r);
                    rightMatches[m.Index].Add(r);
                }
            }
        }

    }
}