using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f79bc)]
    internal class HeightSystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_HEIGHT";

        public ChargenStages Stage => ChargenStages.CG_Stage_Height;

        public WidgetContainer Container { get; }

        [TempleDllLocation(0x10C43168)]
        private readonly WidgetText _minHeightLabel;

        [TempleDllLocation(0x10C42E80)]
        private readonly WidgetText _maxHeightLabel;

        [TempleDllLocation(0x10C42EB0)]
        private readonly WidgetText _currentHeightLabel;

        private readonly HeightSlider _slider;

        private bool chargenHeightActivated;

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10C42E24)]
        private int _minWeight;

        [TempleDllLocation(0x10C42E08)]
        private int _maxWeight;

        [TempleDllLocation(0x10c42e1c)]
        private int _minHeightInches;

        [TempleDllLocation(0x10c4315c)]
        private int _maxHeightInches;

        [TempleDllLocation(0x10189b60)]
        public HeightSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/height_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            _minHeightLabel = doc.GetTextContent("minHeightLabel");
            _maxHeightLabel = doc.GetTextContent("maxHeightLabel");
            _currentHeightLabel = doc.GetTextContent("currentHeightLabel");

            _slider = new HeightSlider();
            _slider.SetPos(doc.GetContainer("sliderContainer").GetPos());
            Container.Add(_slider);
            _slider.OnValueChanged += (newValue) => { UpdateModelScale(); };
        }

        [TempleDllLocation(0x101892b0)]
        public void Activate()
        {
            chargenHeightActivated = true;
        }

        [TempleDllLocation(0x101896c0)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            if (pkt == _pkt)
            {
                chargenHeightActivated = false;
            }

            _pkt = pkt;
            _pkt.modelScale = 0;
            _pkt.height = 0;
            _pkt.weight = 0;
            _slider.Value = 0;

            UpdateModelScale();
        }

        [TempleDllLocation(0x10189700)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:115")]
        public void Show()
        {
            var race = _pkt.raceId.GetValueOrDefault(RaceId.human);
            var gender = _pkt.genderId.GetValueOrDefault(Gender.Male);

            var minHeight = D20RaceSystem.GetMinHeight(race, gender);
            var maxHeight = D20RaceSystem.GetMaxHeight(race, gender);
            SetHeightRange(minHeight, maxHeight);

            _minWeight = D20RaceSystem.GetMinWeight(race, gender);
            _maxWeight = D20RaceSystem.GetMaxWeight(race, gender);

            UpdateModelScale();

            Container.Visible = true;
        }

        [TempleDllLocation(0x10189350)]
        private void SetHeightRange(int minHeightInches, int maxHeightInches)
        {
            _minHeightInches = minHeightInches;
            _maxHeightInches = maxHeightInches;
            _minHeightLabel.SetText(FormatHeight(minHeightInches));
            _minHeightLabel.X = _slider.X - _minHeightLabel.GetPreferredSize().Width;
            _maxHeightLabel.SetText(FormatHeight(maxHeightInches));
            _maxHeightLabel.X = _slider.X - _maxHeightLabel.GetPreferredSize().Width;

            _pkt.height = Math.Clamp(_pkt.height, minHeightInches, maxHeightInches);
            _slider.Value = (_pkt.height - minHeightInches) / (float) (maxHeightInches - minHeightInches);
        }

        private static string FormatHeight(int heightInches)
        {
            var feet = heightInches / 12;
            var inches = heightInches % 12;
            return $"{feet}'{inches}\"";
        }

        [TempleDllLocation(0x101892e0)]
        public bool CheckComplete()
        {
            return chargenHeightActivated;
        }

        [TempleDllLocation(0x101892f0)]
        public void Finalize(CharEditorSelectionPacket pkt, ref GameObjectBody playerObj)
        {
            playerObj.SetInt32(obj_f.critter_height, pkt.height);
            playerObj.SetInt32(obj_f.critter_weight, pkt.weight);
            playerObj.SetInt32(obj_f.model_scale, (int) (pkt.modelScale * 100.0f));
        }

        [TempleDllLocation(0x10189530)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp")]
        private void UpdateModelScale()
        {
            var race = _pkt.raceId.GetValueOrDefault(RaceId.human);
            var gender = _pkt.genderId.GetValueOrDefault(Gender.Male);
            var raceModelScale = D20RaceSystem.GetModelScale(race, gender);

            var height = (int) Math.Round(_minHeightInches + _slider.Value * (_maxHeightInches - _minHeightInches));
            _pkt.height = Math.Clamp(height, _minHeightInches, _maxHeightInches);

            _pkt.modelScale = _pkt.height * raceModelScale / _maxHeightInches;

            _currentHeightLabel.SetText(FormatHeight(_pkt.height));

            // Center the text vertically at the current slider's Y position
            var textHeight = _currentHeightLabel.GetPreferredSize().Height;
            _currentHeightLabel.Y = _slider.ThumbCenterY - textHeight / 2 + 5;

            var heightFactor = (_pkt.height - _minHeightInches) / (float) (_maxHeightInches - _minHeightInches);
            _pkt.weight = (int) (_minWeight + heightFactor * (_maxWeight - _minWeight));

            // Update the model scale for preview purposes
            var editedChar = UiSystems.PCCreation.EditedChar;
            editedChar?.SetInt32(obj_f.model_scale, (int) (_pkt.modelScale * 100.0));
        }

        public bool CompleteForTesting(Dictionary<string, object> props)
        {
            _slider.Value = (float) new Random().NextDouble();
            UpdateModelScale();
            return true;
        }
    }
}