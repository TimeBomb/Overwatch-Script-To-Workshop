using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deltin.Deltinteger.Elements;

// !
// TODO: remove
// !

namespace Deltin.Deltinteger.Parse
{
    public class WhileBuilder
    {
        protected ActionSet ActionSet { get; }
        public bool WasSetup { get; private set; }
        // Start of the loop, using End to mark the end that the continue skip will jump to.
        private SkipEndMarker LoopStart { get; set; }
        private SkipStartMarker LoopSkipStart { get; set; }
        public IWorkshopTree Condition { get; protected set; }
        private Func<IWorkshopTree> GetCondition { get; }

        public WhileBuilder(ActionSet actionSet, IWorkshopTree condition)
        {
            ActionSet = actionSet;
            Condition = condition;
        }
        public WhileBuilder(ActionSet actionSet, Func<IWorkshopTree> getCondition)
        {
            ActionSet = actionSet;
            GetCondition = getCondition;
        }
        protected WhileBuilder(ActionSet actionSet)
        {
            ActionSet = actionSet;
        }

        public virtual void Setup()
        {
            if (WasSetup) throw new Exception("Pattern builder already set up.");
            WasSetup = true;
            ActionSet.ContinueSkip.Setup(ActionSet);

            LoopStart = new SkipEndMarker();
            ActionSet.AddAction(LoopStart);

            if (GetCondition != null)
                Condition = GetCondition();

            LoopSkipStart = new SkipStartMarker(ActionSet, Condition);
            ActionSet.AddAction(LoopSkipStart);
        }

        public virtual void Finish()
        {
            if (!WasSetup) throw new Exception("Pattern builder not set up yet.");
            ActionSet.ContinueSkip.SetSkipCount(ActionSet, LoopStart);
            ActionSet.AddAction(Element.Part<A_Loop>());
            ActionSet.ContinueSkip.ResetSkipCount(ActionSet);

            SkipEndMarker loopEnd = new SkipEndMarker();
            ActionSet.AddAction(loopEnd);
            //LoopSkipStart.SkipCount = LoopSkipStart.GetSkipCount(loopEnd);
        }
    }

    public class ForeachBuilder : WhileBuilder
    {
        private IndexReference IndexStore { get; }
        public IWorkshopTree Array { get; }
        public Element Index { get; }
        public Element IndexValue { get; }

        public ForeachBuilder(ActionSet actionSet, IWorkshopTree array) : base(actionSet)
        {
            IndexStore = actionSet.VarCollection.Assign("foreachIndex,", actionSet.IsGlobal, true);
            Array = array;
            Condition = new V_Compare(IndexStore.GetVariable(), Operators.LessThan, Element.Part<V_CountOf>(Array));
            Index = (Element)IndexStore.GetVariable();
            IndexValue = Element.Part<V_ValueInArray>(Array, IndexStore.GetVariable());
        }

        public override void Setup()
        {
            ActionSet.AddAction(IndexStore.SetVariable(0));
            base.Setup();
        }

        public override void Finish()
        {
            ActionSet.AddAction(IndexStore.ModifyVariable(Operation.Add, 1));
            base.Finish();
        }
    }

    public class SpinWhileBuilder
    {
        public static void Build(ActionSet actionSet, IWorkshopTree spinUntil)
        {
            actionSet.ContinueSkip.Setup(actionSet);
            SkipEndMarker spinAt = new SkipEndMarker();
            actionSet.ContinueSkip.SetSkipCount(actionSet, spinAt);
            actionSet.AddAction(Element.Part<A_LoopIf>(spinUntil));
            actionSet.ContinueSkip.ResetSkipCount(actionSet);
        }
    }
}