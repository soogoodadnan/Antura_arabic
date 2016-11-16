using System.Collections.Generic;

namespace EA4S.Assessment
{
    internal class DefaultLogicInjector : ILogicInjector
    {
        private int AnswerSetNumber = 0; // used to determine if answer is correct or not
        private IDragManager dragManager = null;

        public DefaultLogicInjector( IDragManager dragManager)
        {
            this.dragManager = dragManager;
            ResetRound();
        }

        List< PlaceholderBehaviour> placeholdersList;
        List< AnswerBehaviour> answersList;

        private void ResetRound()
        {
            placeholdersList = new List< PlaceholderBehaviour>();
            answersList = new List< AnswerBehaviour>();
            dragManager.ResetRound();
            AnswerSetNumber = 0;
        }

        /// <summary>
        /// Should return true when it's time to reset round.
        /// </summary>
        public bool AllAnswersCorrect()
        {
            return dragManager.AllAnswered();
        }

        public void EnableGamePlay()
        {
            dragManager.Enable();
        }

        public void Wire( IQuestion question, IAnswer[] answers)
        {
            AnswerSetNumber++; // This one is a new question

            WireQuestion( question);
            WirePlaceHolders( question);
            WireAnswers( answers);

            dragManager.AddElements( placeholdersList, answersList);
        }

        private void WireQuestion( IQuestion question)
        {
            // Add Cover/Uncover for letter shape and audio pronunciation
            // Logic injector complete apart that
        }

        private void WireAnswers( IAnswer[] answers)
        {
            foreach (var a in answers)
            {
                var behaviour = a.gameObject.GetComponent< AnswerBehaviour>();
                answersList.Add(behaviour);
                if (a.IsCorrect())
                {
                    behaviour.GetAnswer().SetAnswerSet( AnswerSetNumber);
                }
                else
                {
                    behaviour.GetAnswer().SetAnswerSet( 0);
                }
            }
        }

        private void WirePlaceHolders( IQuestion question)
        {
            foreach ( var p in question.GetPlaceholders())
            {
                var behaviour = p.GetComponent< PlaceholderBehaviour>();
                behaviour.Placeholder = new DragNDropPlaceholder();
                behaviour.Placeholder.SetAnswer( AnswerSetNumber);
                placeholdersList.Add( behaviour);
            }
        }
    }
}
