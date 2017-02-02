using EA4S.Helpers;
using EA4S.LivingLetters;
using EA4S.MinigamesAPI;
using Kore.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EA4S.Assessment
{
    /// <summary>
    /// Question Generator for asessments that show Image
    /// </summary>
    public class ImageQuestionGenerator : IQuestionGenerator
    {
        private IQuestionProvider provider;
        private QuestionGeneratorState state;
        private IQuestionPack currentPack;

        private bool missingLetter;

        public ImageQuestionGenerator(  IQuestionProvider provider , bool missingLetter, 
                                        AssessmentDialogues dialogues,
                                        AssessmentEvents events)
        {
            this.provider = provider;
            this.missingLetter = missingLetter;
            this.dialogues = dialogues;

            if(AssessmentOptions.Instance.CompleteWordOnAnswered)
                events.OnAllQuestionsAnswered = CompleteWordCoroutine;

            if (AssessmentOptions.Instance.ShowFullWordOnAnswered)
                events.OnAllQuestionsAnswered = ShowFullWordCoroutine;

            state = QuestionGeneratorState.Uninitialized;
            ClearCache();
        }

        private IEnumerator ShowFullWordCoroutine()
        {
            cacheFullWordDataLL.Poof( ElementsSize.PoofOffset);
            cacheFullWordDataLL.Initialize( cacheFullWordData);
            yield return Wait.For( AssessmentOptions.Instance.TimeToShowCompleteWord);
        }

        string cacheCompleteWord = null;
        LetterObjectView cacheCompleteWordLL = null;

        private IEnumerator CompleteWordCoroutine()
        {
            cacheCompleteWordLL.Poof( ElementsSize.PoofOffset);
            cacheCompleteWordLL.Label.text = cacheCompleteWord;
            yield return Wait.For( AssessmentOptions.Instance.TimeToShowCompleteWord);
        }

        public void InitRound()
        {
            if (state != QuestionGeneratorState.Uninitialized && state != QuestionGeneratorState.Completed)
                throw new InvalidOperationException( "Cannot initialized");

            state = QuestionGeneratorState.Initialized;
            ClearCache();
        }

        private void ClearCache()
        {
            totalAnswers = new List< Answer>();
            totalQuestions = new List< IQuestion>();
            partialAnswers = null;
        }

        public void CompleteRound()
        {
            if (state != QuestionGeneratorState.Initialized)
                throw new InvalidOperationException( "Not Initialized");

            state = QuestionGeneratorState.Completed;
        }

        public Answer[] GetAllAnswers()
        {
            if (state != QuestionGeneratorState.Completed)
                throw new InvalidOperationException( "Not Completed");

            return totalAnswers.ToArray();
        }

        public IQuestion[] GetAllQuestions()
        {
            if (state != QuestionGeneratorState.Completed)
                throw new InvalidOperationException( "Not Completed");

            return totalQuestions.ToArray();
        }

        public Answer[] GetNextAnswers()
        {
            if (state != QuestionGeneratorState.QuestionFeeded)
                throw new InvalidOperationException( "Not Initialized");

            state = QuestionGeneratorState.Initialized;
            return partialAnswers;
        }

        List< Answer> totalAnswers;
        List< IQuestion> totalQuestions;
        Answer[] partialAnswers;

        public IQuestion GetNextQuestion()
        {
            if (state != QuestionGeneratorState.Initialized)
                throw new InvalidOperationException( "Not Initialized");

            state = QuestionGeneratorState.QuestionFeeded;

            currentPack = provider.GetNextQuestion();

            List< Answer> answers = new List< Answer>();
            ILivingLetterData questionData = currentPack.GetQuestion();

            //____________________________________
            //Prepare answers for next method call
            //____________________________________

            if ( missingLetter)
            {
                // ### MISSING LETTER ###
                foreach (var wrong in currentPack.GetWrongAnswers())
                {
                    var wrongAnsw = GenerateWrongAnswer( wrong);

                    answers.Add( wrongAnsw);
                    totalAnswers.Add( wrongAnsw);
                }

                var correct = currentPack.GetCorrectAnswers().ToList()[ 0];
                var correctAnsw = GenerateCorrectAnswer( correct);

                answers.Add( correctAnsw);
                totalAnswers.Add( correctAnsw);

                partialAnswers = answers.ToArray();

                // Generate the question
                var question = GenerateMissingLetterQuestion( questionData, correct);
                totalQuestions.Add( question);
                GeneratePlaceHolder( question);
                return question;
            }
            else
            {
                // ### ORDER LETTERS ###
                foreach (var correct in currentPack.GetCorrectAnswers())
                {
                    var correctAnsw = GenerateCorrectAnswer( correct);
                    answers.Add( correctAnsw);
                    totalAnswers.Add( correctAnsw);
                }

                partialAnswers = answers.ToArray();

                // Generate the question
                var question = GenerateQuestion(questionData);
                totalQuestions.Add(question);

                return question;
            }
        }

        private LL_WordData cacheFullWordData;
        private LetterObjectView cacheFullWordDataLL;

        private IQuestion GenerateQuestion( ILivingLetterData data)
        {
            cacheFullWordData = new LL_WordData( data.Id);

            if (AssessmentOptions.Instance.ShowQuestionAsImage)
                data = new LL_ImageData( data.Id);

            cacheFullWordDataLL = LivingLetterFactory.Instance.SpawnQuestion( data);
            return new DefaultQuestion( cacheFullWordDataLL, 0, dialogues);
        }

        private const string RemovedLetterChar = "_";
        private AssessmentDialogues dialogues;

        private IQuestion GenerateMissingLetterQuestion( ILivingLetterData data, ILivingLetterData letterToRemove)
        {
            var imageData = new LL_ImageData( data.Id);
            LL_WordData word = (LL_WordData)data;
            LL_LetterData letter = (LL_LetterData)letterToRemove;

            cacheCompleteWord = word.TextForLivingLetter;

            string text = ArabicAlphabetHelper.GetWordWithMissingLetterText( word.Data, letter.Data, RemovedLetterChar);

            //Spawn word, then replace text with text with missing letter
            var wordGO = LivingLetterFactory.Instance.SpawnQuestion( word);
            wordGO.Label.text = text;
            cacheCompleteWordLL = wordGO;

            var collider = wordGO.GetComponent< BoxCollider>();
            collider.center = new Vector3( 1.5f, 0, 0);
            collider.size = new Vector3( 7.5f, 3, 1.5f);

            return new ImageQuestion( wordGO, imageData, dialogues);
        }

        private Answer GenerateWrongAnswer( ILivingLetterData wrongAnswer)
        {
            return
            LivingLetterFactory.Instance.SpawnAnswer( wrongAnswer, false, dialogues);
        }

        private void GeneratePlaceHolder( IQuestion question)
        {
            var placeholder = LivingLetterFactory.Instance.SpawnCustomElement( CustomElement.Placeholder).transform;
            placeholder.localPosition = new Vector3( 0, 5, 0);
            placeholder.localScale = Vector3.zero;
            question.TrackPlaceholder( placeholder.gameObject);
        }

        private Answer GenerateCorrectAnswer( ILivingLetterData correctAnswer)
        {
            return
            LivingLetterFactory.Instance.SpawnAnswer( correctAnswer, true, dialogues);
        }
    }
}