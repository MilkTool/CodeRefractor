﻿#region Uses

using CodeRefractor.FrontEnd.SimpleOperations.Identifiers;
using CodeRefractor.MiddleEnd.SimpleOperations;

#endregion

namespace CodeRefractor.FrontEnd.SimpleOperations.Casts
{
    public class ClassCasting : LocalOperation
    {
        public ClassCasting()
            : base(OperationKind.CastClass)
        {
        }

        public IdentifierValue Value { get; set; }
        public LocalVariable AssignedTo { get; set; }
    }
}