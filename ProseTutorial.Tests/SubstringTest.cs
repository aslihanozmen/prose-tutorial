﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Learning.Logging;

namespace ProseTutorial
{
    [TestClass]
    public class SubstringTest
    {
        [TestMethod]
        public void TestLearnSubstringPositiveAbsPos()
        {
            //set up the grammar 
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var input = State.CreateForExecution(grammar.Value.InputSymbol, "19-Feb-1960");
            var examples = new Dictionary<State, object> { { input, "Feb" } };
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            var learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            var programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(input) as string;
            Assert.AreEqual("Feb", output);
        }

        [TestMethod]
        public void TestLearnSubstringPositiveAbsPosSecOcurrence() {
            //set up the grammar 
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var firstInput = State.CreateForExecution(grammar.Value.InputSymbol, "16-Feb-2016");
            var secondInput = State.CreateForExecution(grammar.Value.InputSymbol, "14-Jan-2012");
            var examples = new Dictionary<State, object> { { firstInput, "16" }, { secondInput, "12" } };
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            var learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            var programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(firstInput) as string;
            Assert.AreEqual("16", output);
            output = programs.First().Invoke(secondInput) as string;
            Assert.AreEqual("12", output);
        }

        [TestMethod]
        public void TestLearnSubstringNegativeAbsPos() {
            //set up the grammar 
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            //create the examples
            var firstInput = State.CreateForExecution(grammar.Value.InputSymbol, "(Gustavo Soares)");
            var secondInput = State.CreateForExecution(grammar.Value.InputSymbol, "(Titus Barik)");
            var examples = new Dictionary<State, object> { { firstInput, "Gustavo Soares"}, { secondInput, "Titus Barik"}};
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            var learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            var programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(firstInput) as string;
            Assert.AreEqual("Gustavo Soares", output);
            output = programs.First().Invoke(secondInput) as string;
            Assert.AreEqual("Titus Barik", output);
        }

        [TestMethod]
        public void TestLearnSubstringNegativeAbsPosRanking() {
            //set up the grammar 
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var firstInput = State.CreateForExecution(grammar.Value.InputSymbol, "(Gustavo Soares)");
            var examples = new Dictionary<State, object> { { firstInput, "Gustavo Soares" }};
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            var learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            var scoreFeature = new RankingScore(grammar.Value);
            var topPrograms = prose.LearnGrammarTopK(spec, scoreFeature, 1, null);
            var topProgram = topPrograms.First();

            var output = topProgram.Invoke(firstInput) as string;
            Assert.AreEqual("Gustavo Soares", output);
            var secondInput = State.CreateForExecution(grammar.Value.InputSymbol, "(Titus Barik)");
            output = topProgram.Invoke(secondInput) as string;
            Assert.AreEqual("Titus Barik", output);
        }

        [TestMethod]
        public void TestLearnSubstringTwoExamples()
        {
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            var firstInput = State.CreateForExecution(grammar.Value.InputSymbol, "Gustavo Soares");
            var secondInput = State.CreateForExecution(grammar.Value.InputSymbol, "Sumit Gulwani");
            var examples = new Dictionary<State, object> { { firstInput, "Soares" }, { secondInput, "Gulwani" } };
            var spec = new ExampleSpec(examples);

            var learnedSet = prose.LearnGrammar(spec);
            var programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(firstInput) as string;
            Assert.AreEqual("Soares", output);
            var output2 = programs.First().Invoke(secondInput) as string;
            Assert.AreEqual("Gulwani", output2);
        }

        [TestMethod]
        public void TestLearnSubstringOneExample()
        {
            var grammar = DSLCompiler.
                ParseGrammarFromFile("../../../ProseTutorial/grammar/substring.grammar");
            var prose = ConfigureSynthesis(grammar.Value);

            var input = State.CreateForExecution(grammar.Value.InputSymbol, "Gustavo Soares");
            var examples = new Dictionary<State, object> { { input, "Soares" }};

            var spec = new ExampleSpec(examples);

            var scoreFeature = new RankingScore(grammar.Value);
            var topPrograms = prose.LearnGrammarTopK(spec, scoreFeature, 1, null);
            var topProgram = topPrograms.First();
            var output = topProgram.Invoke(input) as string;
            Assert.AreEqual("Soares", output);

            var input2 = State.CreateForExecution(grammar.Value.InputSymbol, "Sumit Gulwani");
            var output2 = topProgram.Invoke(input2) as string;
            Assert.AreEqual("Gulwani", output2);
        }

        public static SynthesisEngine ConfigureSynthesis(Grammar grammar)
        {
            var witnessFunctions = new WitnessFunctions(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }
    }
}