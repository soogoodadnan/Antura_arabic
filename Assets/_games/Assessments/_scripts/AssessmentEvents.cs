using System.Collections;

namespace EA4S.Assessment
{
    public class AssessmentEvents
    {
        public delegate IEnumerator CoroutineEvent();

        public CoroutineEvent OnAllQuestionsAnswered = null;

        public IEnumerator NoEvent()
        {
            yield return null;
        }
    }
}