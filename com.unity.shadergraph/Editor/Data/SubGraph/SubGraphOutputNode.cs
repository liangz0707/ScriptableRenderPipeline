using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph
{
    class SubGraphOutputNode : AbstractMaterialNode, IHasSettings
    {
        [SerializeField]
        List<ReorderableSlot> m_InputSlots = new List<ReorderableSlot>() {};

		public List<ReorderableSlot> inputSlots
		{
			get 
            { 
                if(m_InputSlots == null)
                    m_InputSlots = new List<ReorderableSlot>();

                return m_InputSlots; 
            }
		}

        public SubGraphOutputNode()
        {
            name = "Sub Graph Outputs";
        }

        public override bool hasPreview
        {
            get { return false; }
        }

        public ShaderStageCapability effectiveShaderStage
        {
            get
            {
                List<MaterialSlot> slots = new List<MaterialSlot>();
                GetInputSlots(slots);

                foreach(MaterialSlot slot in slots)
                {
                    ShaderStageCapability stage = NodeUtils.GetEffectiveShaderStageCapability(slot, true);

                    if(stage != ShaderStageCapability.All)
                        return stage;
                }

                return ShaderStageCapability.All;
            }
        }

        private void ValidateShaderStage()
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetInputSlots(slots);

            foreach(MaterialSlot slot in slots)
                slot.stageCapability = ShaderStageCapability.All;

            var effectiveStage = effectiveShaderStage;

            foreach(MaterialSlot slot in slots)
                slot.stageCapability = effectiveStage;
        }

        public override void ValidateNode()
        {
            List<int> validSlots = new List<int>();
            ReorderableSlotListUtil.UpdateSlotList(this, inputSlots, ref validSlots);
            RemoveSlotsNameNotMatching(validSlots);

            ValidateShaderStage();
            base.ValidateNode();
        }

        public virtual int AddSlot()
        {
            var index = this.GetInputSlots<ISlot>().Count() + 1;
            AddSlot(new Vector4MaterialSlot(index, "Output " + index, "Output" + index, SlotType.Input, Vector4.zero));
            return index;
        }

        public void RemapOutputs(ShaderGenerator visitor, GenerationMode generationMode)
        {
            foreach (var slot in graphOutputs)
                visitor.AddShaderChunk(string.Format("{0} = {1};", slot.shaderOutputName, GetSlotValue(slot.id, generationMode)), true);
        }

        public IEnumerable<MaterialSlot> graphOutputs
        {
            get
            {
                return NodeExtensions.GetInputSlots<MaterialSlot>(this).OrderBy(x => x.id);
            }
        }

        public VisualElement CreateSettingsElement()
        {
            PropertySheet ps = new PropertySheet();
            ps.style.width = 362;
            ps.Add(new ReorderableSlotListView(this, inputSlots, SlotType.Input));
            return ps;
        }
    }
}
