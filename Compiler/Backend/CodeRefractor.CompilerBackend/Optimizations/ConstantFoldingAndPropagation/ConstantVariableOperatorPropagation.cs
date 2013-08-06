#region Usings

using CodeRefractor.RuntimeBase.MiddleEnd;
using CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations;
using CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations.Identifiers;
using CodeRefractor.RuntimeBase.MiddleEnd.SimpleOperations.Operators;

#endregion

namespace CodeRefractor.CompilerBackend.Optimizations.ConstantFoldingAndPropagation
{
    public class ConstantVariableOperatorPropagation : ConstantVariablePropagationBase
    {
        public override void OptimizeOperations(MetaMidRepresentation intermediateCode)
        {
            var operations = intermediateCode.LocalOperations;
            for (var i = 0; i < operations.Count - 1; i++)
            {
                Assignment srcVariableDefinition;
                var constValue = GetConstantFromOperation(operations, i, out srcVariableDefinition);
                if (constValue == null)
                    continue;

                for (var j = i + 1; j < operations.Count; j++)
                {
                    var destOperation = operations[j];
                    if (destOperation.Kind == LocalOperation.Kinds.Label)
                        break;
                    if (destOperation.Kind == LocalOperation.Kinds.BranchOperator)
                        break;
                    if (destOperation.Kind != LocalOperation.Kinds.BinaryOperator &&
                        destOperation.Kind != LocalOperation.Kinds.UnaryOperator) continue;
                    var destAssignment = (OperatorBase) destOperation.Value;
                    if (SameVariable(destAssignment.AssignedTo, srcVariableDefinition.Left))
                        break;

                    var rightBinaryAssignment = destAssignment as BinaryOperator;
                    var unaryAssignment = destAssignment as UnaryOperator;

                    if (unaryAssignment != null) continue;
                    if (rightBinaryAssignment == null) continue;
                    if (SameVariable(rightBinaryAssignment.Left as LocalVariable, srcVariableDefinition.Left))
                    {
                        rightBinaryAssignment.Left = constValue;
                        Result = true;
                        continue;
                    }
                    if (SameVariable(rightBinaryAssignment.Right as LocalVariable, srcVariableDefinition.Left))
                    {
                        rightBinaryAssignment.Right = constValue;
                        Result = true;
                        continue;
                    }
                }
            }
        }
    }
}