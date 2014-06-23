#region Usings

using System;
using CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations.Identifiers;

#endregion

namespace CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations
{
    public class Assignment : LocalOperation, IClonableOperation
    {
        public LocalVariable AssignedTo;
        public IdentifierValue Right;

        public Assignment()
            : base(OperationKind.Assignment)
        {
        }

        public override string ToString()
        {
            return String.Format("{0} = {1}", AssignedTo.Name, Right);
        }

        public object Clone()
        {
            return new Assignment
            {
                AssignedTo = (LocalVariable) AssignedTo.Clone(),
                Right = Right.Clone()
            };
        }
    }
}