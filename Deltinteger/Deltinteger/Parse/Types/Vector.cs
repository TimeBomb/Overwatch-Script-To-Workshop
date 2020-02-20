using Deltin.Deltinteger.Elements;
using Deltin.Deltinteger.CustomMethods;
using Deltin.Deltinteger.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Deltin.Deltinteger.Parse;

namespace Deltin.Deltinteger.Parse
{
    public class VectorType : CodeType
    {
        private Scope objectScope = new Scope("Vector");
        private Scope staticScope = new Scope("Vector");

        private InternalVar X;
        private InternalVar Y;
        private InternalVar Z;

        private InternalVar HorizontalAngle;
        private InternalVar VerticalAngle;

        private InternalVar Zero;

        public VectorType() : base("Vector")
        {
            CanBeDeleted = false;
            CanBeExtended = false;

            X = CreateInternalVar("X", "The X component of the vector.");
            Y = CreateInternalVar("Y", "The Y component of the vector.");
            Z = CreateInternalVar("Z", "The Z component of the vector.");
            HorizontalAngle = CreateInternalVar("HorizontalAngle", "The horizontal angle of the vector.");
            VerticalAngle = CreateInternalVar("VerticalAngle", "The vertical angle of the vector.");
            Zero = CreateInternalVar("Zero", "Equal to `Vector(0, 0, 0)`.", true);

            objectScope.AddNativeMethod(CustomMethodData.GetCustomMethod<DistanceTo>());
            objectScope.AddNativeMethod(CustomMethodData.GetCustomMethod<CrossProduct>());
            objectScope.AddNativeMethod(CustomMethodData.GetCustomMethod<Normalize>());
            objectScope.AddNativeMethod(CustomMethodData.GetCustomMethod<DirectionTowards>());
        }

        private InternalVar CreateInternalVar(string name, string documentation, bool isStatic = false)
        {
            // Create the variable.
            InternalVar newInternalVar = new InternalVar(name, CompletionItemKind.Property);

            // Make the variable unsettable.
            newInternalVar.IsSettable = false;

            // Set the documentation.
            newInternalVar.Documentation = documentation;

            // Add the variable to the object scope.
            if (!isStatic) objectScope.AddNativeVariable(newInternalVar);
            // Add the variable to the static scope.
            else staticScope.AddNativeVariable(newInternalVar);

            return newInternalVar;
        }

        public override void WorkshopInit(DeltinScript translateInfo)
        {
            translateInfo.DefaultIndexAssigner.Add(Zero, new V_Vector());
        }

        public override void AddObjectVariablesToAssigner(IWorkshopTree reference, VarIndexAssigner assigner)
        {
            assigner.Add(X, Element.Part<V_XOf>(reference));
            assigner.Add(Y, Element.Part<V_YOf>(reference));
            assigner.Add(Z, Element.Part<V_ZOf>(reference));

            assigner.Add(HorizontalAngle, Element.Part<V_HorizontalAngleFromDirection>(reference));
            assigner.Add(VerticalAngle, Element.Part<V_VerticalAngleFromDirection>(reference));
        }

        public override Scope GetObjectScope() => objectScope;
        public override Scope ReturningScope() => staticScope;

        public override CompletionItem GetCompletion() => new CompletionItem() {
            Label = Name,
            Kind = CompletionItemKind.Class
        };

        // DistanceTo() method
        [CustomMethod("DistanceTo", "Gets the distance between 2 vectors.", CustomMethodType.Value, false)]
        class DistanceTo : CustomMethodBase
        {
            public override CodeParameter[] Parameters() => new CodeParameter[] {
                new CodeParameter("other", "The vector to get the distance to.")
            };

            public override IWorkshopTree Get(ActionSet actionSet, IWorkshopTree[] parameterValues) => Element.Part<V_DistanceBetween>(actionSet.CurrentObject, parameterValues[0]);
        }

        // CrossProduct() method
        [CustomMethod("CrossProduct", "The cross product of the specified vector.", CustomMethodType.Value, false)]
        class CrossProduct : CustomMethodBase
        {
            public override CodeParameter[] Parameters() => new CodeParameter[] {
                new CodeParameter("other", "The vector to get the cross product to.")
            };

            public override IWorkshopTree Get(ActionSet actionSet, IWorkshopTree[] parameterValues) => Element.Part<V_CrossProduct>(actionSet.CurrentObject, parameterValues[0]);
        }

        // Normalize() method
        [CustomMethod("Normalize", "The unit-length normalization of the vector.", CustomMethodType.Value, false)]
        class Normalize : CustomMethodBase
        {
            public override CodeParameter[] Parameters() => new CodeParameter[0];

            public override IWorkshopTree Get(ActionSet actionSet, IWorkshopTree[] parameterValues) => Element.Part<V_Normalize>(actionSet.CurrentObject);
        }

        // DirectionTowards() method
        [CustomMethod("DirectionTowards", "The unit-length direction vector to another vector.", CustomMethodType.Value, false)]
        class DirectionTowards : CustomMethodBase
        {
            public override CodeParameter[] Parameters() => new CodeParameter[] {
                new CodeParameter("other", "The vector to get the direction towards.")
            };

            public override IWorkshopTree Get(ActionSet actionSet, IWorkshopTree[] parameterValues) => Element.Part<V_DirectionTowards>(actionSet.CurrentObject, parameterValues[0]);
        }
    }
}