
namespace FindDuplicates
{
    public class SetDuplicateEntry : DuplicateEntry
    {
        public SetDuplicateEntry(DuplicateSet set)
        {
            set_ = set;
        }
        public override string ToString()
        {
            return Text();
        }

        public string Text() 
        {
            return "--- " + set_.ViewString();
        }

        public bool IsFile() 
        {
            return false;
        }

        private readonly DuplicateSet set_;
    }
}
