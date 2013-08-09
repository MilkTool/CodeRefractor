using CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations.Identifiers;

namespace CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations.Operators
{
    public class BranchOperator : OperatorBase
    {
        public BranchOperator(string name)
            : base(name)
        {
        }

        public int JumpTo { get; set; }
        public IdentifierValue CompareValue { get; set; }
        public IdentifierValue SecondValue { get; set; }
    }
}