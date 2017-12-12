﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DCS_BIOS;
using HidLibrary;
using System.Threading;

namespace NonVisuals
{
    public class PZ70LCDButtonByteList
    {
        /*
LCD Button Byte
00000000
||||||||_ AP_BUTTON
|||||||_ HDG_BUTTON
||||||_ NAV_BUTTON
|||||_ IAS_BUTTON
||||_ ALT_BUTTON
|||_ VS_BUTTON
||_ APR_BUTTON
|_ REV_BUTTON
 */
        private byte _apMask = 1;
        private byte _hdgMask = 2;
        private byte _navMask = 4;
        private byte _iasMask = 8;
        private byte _altMask = 16;
        private byte _vsMask = 32;
        private byte _aprMask = 64;
        private byte _revMask = 128;
        private int _dialAltMask = 256;
        private int _dialVsMask = 512;
        private int _dialIasMask = 1024;
        private int _dialHdgMask = 2048;
        private int _dialCrsMask = 4096;
        private int _buttonIsOnMask = 8192;

        //bool isSet = (b & mask) != 0
        //Set to 1" b |= mask
        //Set to zero
        //b &= ~mask
        //Toggle
        //b ^= mask
        private byte[] _buttonBytes = new byte[8];
        private int[] _buttonDialPosition = new int[8];

        public PZ70LCDButtonByteList()
        {
            _buttonDialPosition[0] |= _dialAltMask;
            _buttonDialPosition[1] |= _dialVsMask;
            _buttonDialPosition[2] |= _dialIasMask;
            _buttonDialPosition[3] |= _dialHdgMask;
            _buttonDialPosition[4] |= _dialCrsMask;
        }

        public bool FlipButton(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                return FlipButton(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob));
            }
            catch (Exception e)
            {
                Common.LogError(e, "Flipbutton()");
                throw;
            }
        }

        public bool IsOn(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knobs)
        {
            try
            {
                var dialMask = GetMaskForDialPosition(pz70DialPosition);
                var buttonMask = GetMaskForButton(multiPanelPZ70Knobs);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & dialMask) != 0)
                    {
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel IsOn : Failed to find Mask for dial position " + pz70DialPosition + " knob " + multiPanelPZ70Knobs);
            }
            catch (Exception e)
            {
                Common.LogError(e, "IsOn()");
                throw;
            }
        }

        public int GetMaskForDialPosition(PZ70DialPosition pz70DialPosition)
        {
            try
            {

                switch (pz70DialPosition)
                {
                    case PZ70DialPosition.ALT:
                        {
                            return DialAltMask;
                        }
                    case PZ70DialPosition.VS:
                        {
                            return DialVsMask;
                        }
                    case PZ70DialPosition.IAS:
                        {
                            return DialIasMask;
                        }
                    case PZ70DialPosition.HDG:
                        {
                            return DialHdgMask;
                        }
                    case PZ70DialPosition.CRS:
                        {
                            return DialCrsMask;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for dial position " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetMaskForButton(MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                switch (multiPanelPZ70Knob)
                {
                    case MultiPanelPZ70Knobs.AP_BUTTON:
                        {
                            return ApMask;
                        }
                    case MultiPanelPZ70Knobs.HDG_BUTTON:
                        {
                            return HdgMask;
                        }
                    case MultiPanelPZ70Knobs.NAV_BUTTON:
                        {
                            return NavMask;
                        }
                    case MultiPanelPZ70Knobs.IAS_BUTTON:
                        {
                            return IasMask;
                        }
                    case MultiPanelPZ70Knobs.ALT_BUTTON:
                        {
                            return AltMask;
                        }
                    case MultiPanelPZ70Knobs.VS_BUTTON:
                        {
                            return VsMask;
                        }
                    case MultiPanelPZ70Knobs.APR_BUTTON:
                        {
                            return AprMask;
                        }
                    case MultiPanelPZ70Knobs.REV_BUTTON:
                        {
                            return RevMask;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for button " + multiPanelPZ70Knob);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool FlipButton(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] ^= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel FlipButton : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonOnOrOff(int buttonDialMask, byte buttonMask, bool on)
        {
            if (on)
            {
                return SetButtonOn(buttonDialMask, buttonMask);
            }
            return SetButtonOff(buttonDialMask, buttonMask);
        }

        public bool SetButtonOff(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] &= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel ButtonOff : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonOn(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] |= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel ButtonOn : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetButtonByte(PZ70DialPosition pz70DialPosition)
        {
            try
            {
                var mask = GetMaskForDialPosition(pz70DialPosition);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    //(b & mask) != 0
                    if ((_buttonDialPosition[i] & mask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel : Failed to find button byte for " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetButtonByte(int buttonDialMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel GetButtonByte : Failed to find Mask for dial " + buttonDialMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public bool SetButtonByte(PZ70DialPosition pz70DialPosition, MultiPanelKnob multiPanelPZ70Knob)
        {
            return SetButtonOnOrOff(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob.MultiPanelPZ70Knob), multiPanelPZ70Knob.IsOn);
        }

        public byte ApMask => _apMask;

        public byte HdgMask => _hdgMask;

        public byte NavMask => _navMask;

        public byte IasMask => _iasMask;

        public byte AltMask => _altMask;

        public byte VsMask => _vsMask;

        public byte AprMask => _aprMask;

        public byte RevMask => _revMask;

        public int DialAltMask => _dialAltMask;

        public int DialVsMask => _dialVsMask;

        public int DialIasMask => _dialIasMask;

        public int DialHdgMask => _dialHdgMask;

        public int DialCrsMask => _dialCrsMask;

        public int ButtonIsOnMask => _buttonIsOnMask;
    }







    public enum PZ70DialPosition
    {
        ALT = 0,
        VS = 2,
        IAS = 4,
        HDG = 8,
        CRS = 16
    }

    public class MultiPanelPZ70 : SaitekPanel
    {
        private int _lcdKnobSensitivity;
        private volatile byte KnobSensitivitySkipper;
        private HashSet<DCSBIOSBindingPZ70> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ70>();
        private HashSet<DCSBIOSBindingLCDPZ70> _dcsBiosLcdBindings = new HashSet<DCSBIOSBindingLCDPZ70>();
        private HashSet<KnobBindingPZ70> _knobBindings = new HashSet<KnobBindingPZ70>();
        private HashSet<MultiPanelKnob> _multiPanelKnobs = new HashSet<MultiPanelKnob>();
        private bool _isFirstNotification = true;
        private byte[] _oldMultiPanelValue = { 0, 0, 0 };
        private byte[] _newMultiPanelValue = { 0, 0, 0 };
        private PZ70DialPosition _pz70DialPosition = PZ70DialPosition.ALT;

        private readonly object _lcdLockObject = new object();
        private readonly object _lcdDataVariablesLockObject = new object();
        private PZ70LCDButtonByteList _lcdButtonByteListHandler = new PZ70LCDButtonByteList();
        //private volatile int _upperLcdValue = 0;
        //private volatile int _lowerLcdValue = 0;



        private long _doUpdatePanelLCD;

        public MultiPanelPZ70(HIDSkeleton hidSkeleton)
            : base(SaitekPanelsEnum.PZ70MultiPanel, hidSkeleton)
        {
            VendorId = 0x6A3;
            ProductId = 0xD06;
            CreateMultiKnobs();
            Startup();
        }

        public override sealed void Startup()
        {
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("MultiPanelPZ70.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            UpdateCounter(address, data);
            foreach (var dcsbiosBindingLCDPZ70 in _dcsBiosLcdBindings)
            {
                if (!dcsbiosBindingLCDPZ70.UseFormula && dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && address == dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.Address)
                {
                    lock (_lcdDataVariablesLockObject)
                    {
                        var tmp = dcsbiosBindingLCDPZ70.CurrentValue;
                        dcsbiosBindingLCDPZ70.CurrentValue = (int)dcsbiosBindingLCDPZ70.DCSBIOSOutputObject.GetUIntValue(data);
                        if (tmp != dcsbiosBindingLCDPZ70.CurrentValue)
                        {
                            Interlocked.Add(ref _doUpdatePanelLCD, 1);
                        }
                    }
                }
                else if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.UseFormula)
                {
                    if (dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.CheckForMatch(address, data))
                    {
                        lock (_lcdDataVariablesLockObject)
                        {
                            var tmp = dcsbiosBindingLCDPZ70.CurrentValue;
                            dcsbiosBindingLCDPZ70.CurrentValue = dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject.Evaluate();
                            if (tmp != dcsbiosBindingLCDPZ70.CurrentValue)
                            {
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                            }
                        }
                    }
                }
            }
            UpdateLCD();
        }

        public override void ImportSettings(List<string> settings)
        {
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }

            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId) && setting.Contains(SettingsVersion()))
                {
                    if (setting.StartsWith("MultiPanelKnob{"))
                    {
                        var knobBinding = new KnobBindingPZ70();
                        knobBinding.ImportSettings(setting);
                        _knobBindings.Add(knobBinding);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingPZ70 = new DCSBIOSBindingPZ70();
                        dcsBIOSBindingPZ70.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingPZ70);
                    }
                    else if (setting.StartsWith("MultiPanelDCSBIOSControlLCD{"))
                    {
                        var dcsBIOSBindingLCDPZ70 = new DCSBIOSBindingLCDPZ70();
                        dcsBIOSBindingLCDPZ70.ImportSettings(setting);
                        _dcsBiosLcdBindings.Add(dcsBIOSBindingLCDPZ70);
                    }
                }
            }
            OnSettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            var result = new List<string>();

            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.OSKeyPress != null)
                {
                    result.Add(knobBinding.ExportSettings());
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                {
                    result.Add(dcsBiosBinding.ExportSettings());
                }
            }
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.HasBinding)
                {
                    result.Add(dcsBiosBindingLCD.ExportSettings());
                }
            }
            return result;
        }

        public string GetKeyPressForLoggingPurposes(MultiPanelKnob multiPanelKnob)
        {
            var result = "";
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                {
                    result = knobBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        public override void ClearSettings()
        {
            _knobBindings.Clear();
            _dcsBiosBindings.Clear();
            _dcsBiosLcdBindings.Clear();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 3)
            {
                Array.Copy(_newMultiPanelValue, _oldMultiPanelValue, 3);
                Array.Copy(report.Data, _newMultiPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(_oldMultiPanelValue, _newMultiPanelValue);

                //Set _selectedMode and LCD button statuses
                //and performs the actual actions for key presses
                PZ70SwitchChanged(hashSet);
                //Sends event
                OnSwitchesChanged(hashSet);


                _isFirstNotification = false;
                if (Common.DebugOn)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(Convert.ToString(report.Data[i], 2).PadLeft(8, '0') + "  ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing knobs has been changed:\n");
                        foreach (var multiPanelKnob in hashSet)
                        {
                            Common.DebugP(((MultiPanelKnob)multiPanelKnob).MultiPanelPZ70Knob + ", value is " + FlagValue(_newMultiPanelValue, ((MultiPanelKnob)multiPanelKnob)));
                        }
                    }
                }
                Common.DebugP("\r\nDone!\r\n");
            }
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var knobBinding = new KnobBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _knobBindings.Count);
            IsDirtyMethod();
        }

        public void AddOrUpdateSequencedKeyBinding(string information, MultiPanelPZ70Knobs multiPanelPZ70Knob, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveMultiPanelKnobFromList(2, multiPanelPZ70Knob, whenTurnedOn);
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        knobBinding.OSKeyPress = null;
                    }
                    else
                    {
                        knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        knobBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var knobBinding = new KnobBindingPZ70();
                knobBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                knobBinding.DialPosition = _pz70DialPosition;
                knobBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                knobBinding.WhenTurnedOn = whenTurnedOn;
                _knobBindings.Add(knobBinding);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs multiPanelPZ70Knob, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveMultiPanelKnobFromList(1, multiPanelPZ70Knob, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSBindingPZ70();
                dcsBiosBinding.MultiPanelPZ70Knob = multiPanelPZ70Knob;
                dcsBiosBinding.DialPosition = _pz70DialPosition;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutput dcsbiosOutput, PZ70LCDPosition pz70LCDPosition)
        {
            var found = false;
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputObject = dcsbiosOutput;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateLCDBinding(DCSBIOSOutputFormula dcsbiosOutputFormula, PZ70LCDPosition pz70LCDPosition)
        {
            var found = false;
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                    Debug.Print("3 found");
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBindingLCD = new DCSBIOSBindingLCDPZ70();
                dcsBiosBindingLCD.DialPosition = _pz70DialPosition;
                dcsBiosBindingLCD.DCSBIOSOutputFormulaObject = dcsbiosOutputFormula;
                dcsBiosBindingLCD.PZ70LCDPosition = pz70LCDPosition;
                _dcsBiosLcdBindings.Add(dcsBiosBindingLCD);
            }
            IsDirtyMethod();
        }

        public void AddOrUpdateDCSBIOSLcdBinding(PZ70LCDPosition pz70LCDPosition)
        {
            //Removes config
            foreach (var dcsBiosBindingLCD in _dcsBiosLcdBindings)
            {
                if (dcsBiosBindingLCD.DialPosition == _pz70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == pz70LCDPosition)
                {
                    dcsBiosBindingLCD.DCSBIOSOutputObject = null;
                    break;
                }
            }
            IsDirtyMethod();
        }

        private void RemoveMultiPanelKnobFromList(int list, MultiPanelPZ70Knobs multiPanelPZ70Knob, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var knobBindingPZ70 in _knobBindings)
                        {
                            if (knobBindingPZ70.DialPosition == _pz70DialPosition && knobBindingPZ70.MultiPanelPZ70Knob == multiPanelPZ70Knob && knobBindingPZ70.WhenTurnedOn == whenTurnedOn)
                            {
                                knobBindingPZ70.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }


        private void PZ70SwitchChanged(IEnumerable<object> hashSet)
        {
            foreach (var o in hashSet)
            {
                var multiPanelKnob = (MultiPanelKnob)o;
                if (multiPanelKnob.IsOn)
                {
                    switch (multiPanelKnob.MultiPanelPZ70Knob)
                    {
                        case MultiPanelPZ70Knobs.KNOB_ALT:
                            {
                                _pz70DialPosition = PZ70DialPosition.ALT;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                _pz70DialPosition = PZ70DialPosition.VS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                _pz70DialPosition = PZ70DialPosition.IAS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                _pz70DialPosition = PZ70DialPosition.HDG;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                _pz70DialPosition = PZ70DialPosition.CRS;
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                UpdateLCD();
                                break;
                            }
                        case MultiPanelPZ70Knobs.AP_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.AP_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.VS_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.APR_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                multiPanelKnob.IsOn = _lcdButtonByteListHandler.FlipButton(PZ70_DialPosition, MultiPanelPZ70Knobs.REV_BUTTON);
                                Interlocked.Add(ref _doUpdatePanelLCD, 1);
                                break;
                            }
                    }
                }
            }
            UpdateLCD();
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var o in hashSet)
            {
                var multiPanelKnob = (MultiPanelKnob)o;
                /*
                 * IMPORTANT
                 * ---------
                 * The LCD buttons toggle between on and off. It is the toggle value that defines if the button is OFF, not the fact that the user releases the button.
                 * Therefore the forementioned buttons cannot be used as usual in a loop with knobBinding.WhenTurnedOn
                 * Instead the buttons global bool value must be used!
                 * 
                 */
                var found = false;
                foreach (var knobBinding in _knobBindings)
                {
                    if (knobBinding.DialPosition == _pz70DialPosition && knobBinding.OSKeyPress != null && knobBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                    {
                        if (knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC || knobBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                        {
                            if (!SkipCurrentLcdKnobChange())
                            {
                                knobBinding.OSKeyPress.Execute();
                            }
                            found = true;
                        }
                        else
                        {
                            knobBinding.OSKeyPress.Execute();
                            found = true;
                        }
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DialPosition == _pz70DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.MultiPanelPZ70Knob == multiPanelKnob.MultiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == multiPanelKnob.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
            }
        }

        protected bool SkipCurrentLcdKnobChange()
        {
            switch (_lcdKnobSensitivity)
            {
                case 0:
                    {
                        //Do nothing all manipulation is let through
                        break;
                    }
                case -1:
                    {
                        //Skip every 2 manipulations
                        KnobSensitivitySkipper++;
                        if (KnobSensitivitySkipper <= 2)
                        {
                            return true;
                        }
                        KnobSensitivitySkipper = 0;
                        break;
                    }
                case -2:
                    {
                        //Skip every 4 manipulations
                        KnobSensitivitySkipper++;
                        if (KnobSensitivitySkipper <= 4)
                        {
                            return true;
                        }
                        KnobSensitivitySkipper = 0;
                        break;
                    }
            }
            return false;
        }

        public void ClearAllBindings(MultiPanelPZ70KnobOnOff multiPanelPZ70KnobOnOff)
        {
            //This must accept lists
            foreach (var knobBinding in _knobBindings)
            {
                if (knobBinding.MultiPanelPZ70Knob == multiPanelPZ70KnobOnOff.MultiPanelPZ70Knob && knobBinding.WhenTurnedOn == multiPanelPZ70KnobOnOff.On)
                {
                    knobBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.MultiPanelPZ70Knob == multiPanelPZ70KnobOnOff.MultiPanelPZ70Knob && dcsBiosBinding.WhenTurnedOn == multiPanelPZ70KnobOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("MultiPanelPZ70 _knobBindings : " + _knobBindings.Count);
            Common.DebugP("MultiPanelPZ70 _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }


        public void UpdateLCD()
        {
            //345
            //15600
            //
            //[0x0]
            //[1] [2] [3] [4] [5]
            //[6] [7] [8] [9] [10]
            //[11 BUTTONS]


            if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
            {
                return;
            }
            var bytes = new byte[12];
            bytes[0] = 0x0;
            for (var ii = 1; ii < bytes.Length - 1; ii++)
            {
                bytes[ii] = 0xFF;
            }

            bytes[11] = _lcdButtonByteListHandler.GetButtonByte(PZ70_DialPosition);

            bool foundUpperValue = false;
            bool foundLowerValue = false;

            var upperValue = 0;
            var lowerValue = 0;
            lock (_lcdDataVariablesLockObject)
            {
                foreach (var dcsbiosBindingLCDPZ70 in _dcsBiosLcdBindings)
                {
                    if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == PZ70LCDPosition.UpperLCD)
                    {
                        foundUpperValue = true;
                        upperValue = dcsbiosBindingLCDPZ70.CurrentValue;
                    }
                    if (dcsbiosBindingLCDPZ70.DialPosition == _pz70DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == PZ70LCDPosition.LowerLCD)
                    {
                        foundLowerValue = true;
                        lowerValue = dcsbiosBindingLCDPZ70.CurrentValue;
                    }
                }
            }

            if (foundUpperValue)
            {
                if (upperValue < 0)
                {
                    upperValue = Math.Abs(upperValue);
                }
                var dataAsString = upperValue.ToString();

                var i = dataAsString.Length;
                var arrayPosition = 5;
                do
                {
                    //    3 0 0
                    //1 5 6 0 0
                    //1 2 3 4 5    
                    bytes[arrayPosition] = (byte)dataAsString[i - 1];
                    arrayPosition--;
                    i--;
                } while (i > 0);
            }
            if (foundLowerValue)
            {
                //Important!
                //Lower LCD will show a dash "-" for 0xEE.
                //Smallest negative value that can be shown is -9999
                //Largest positive value that can be shown is 99999
                if (lowerValue < -9999)
                {
                    lowerValue = -9999;
                }
                var dataAsString = lowerValue.ToString();

                var i = dataAsString.Length;
                var arrayPosition = 10;
                do
                {
                    //    3 0 0
                    //1 5 6 0 0
                    //1 2 3 4 5    
                    var s = dataAsString[i - 1];
                    if (s == '-')
                    {
                        bytes[arrayPosition] = 0xEE;
                    }
                    else
                    {
                        bytes[arrayPosition] = (byte)s;
                    }
                    arrayPosition--;
                    i--;
                } while (i > 0);
            }
            lock (_lcdLockObject)
            {
                SendLEDData(bytes);
            }
            Interlocked.Add(ref _doUpdatePanelLCD, -1);
        }

        public void SendLEDData(byte[] array)
        {
            try
            {
                if (HIDSkeletonBase.HIDWriteDevice != null)
                {
                    Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDWriteDevice.WriteFeatureData(array);
                }
            }
            catch (Exception e)
            {
                Common.DebugP("SendLEDData() :\n" + e.Message + e.StackTrace);
                SetLastException(e);
            }
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();
            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var multiPanelKnob in _multiPanelKnobs)
                {
                    if (multiPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, multiPanelKnob.Mask) || _isFirstNotification))
                    {
                        multiPanelKnob.IsOn = FlagValue(newValue, multiPanelKnob);
                        //Do not add OFF signals for LCD buttons. They are not used. The OFF value is a TOGGLE value and respective button's bool value must be read instead.
                        if (!multiPanelKnob.IsOn)
                        {
                            switch (multiPanelKnob.MultiPanelPZ70Knob)
                            {
                                case MultiPanelPZ70Knobs.AP_BUTTON:
                                case MultiPanelPZ70Knobs.HDG_BUTTON:
                                case MultiPanelPZ70Knobs.NAV_BUTTON:
                                case MultiPanelPZ70Knobs.IAS_BUTTON:
                                case MultiPanelPZ70Knobs.ALT_BUTTON:
                                case MultiPanelPZ70Knobs.VS_BUTTON:
                                case MultiPanelPZ70Knobs.APR_BUTTON:
                                case MultiPanelPZ70Knobs.REV_BUTTON:
                                    {
                                        //Do not add OFF values for these buttons! Read comment above.
                                        break;
                                    }
                                default:
                                    {
                                        result.Add(multiPanelKnob);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            result.Add(multiPanelKnob);
                        }
                    }
                }
            }
            return result;
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        private void CreateMultiKnobs()
        {
            _multiPanelKnobs = MultiPanelKnob.GetMultiPanelKnobs();
        }

        private static bool FlagValue(byte[] currentValue, MultiPanelKnob multiPanelKnob)
        {
            return (currentValue[multiPanelKnob.Group] & multiPanelKnob.Mask) > 0;
        }

        private void DeviceAttachedHandler()
        {
            Startup();
            //IsAttached = true;
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
            //IsAttached = false;
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public HashSet<DCSBIOSBindingPZ70> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }

        public HashSet<KnobBindingPZ70> KeyBindings
        {
            get { return _knobBindings; }
            set { _knobBindings = value; }
        }

        public HashSet<MultiPanelKnob> MultiPanelKnobs
        {
            get { return _multiPanelKnobs; }
            set { _multiPanelKnobs = value; }
        }

        public HashSet<KnobBindingPZ70> KeyBindingsHashSet
        {
            get { return _knobBindings; }
            set { _knobBindings = value; }
        }

        public HashSet<DCSBIOSBindingLCDPZ70> LCDBindings
        {
            get { return _dcsBiosLcdBindings; }
            set { _dcsBiosLcdBindings = value; }
        }

        public int LCDKnobSensitivity
        {
            get { return _lcdKnobSensitivity; }
            set { _lcdKnobSensitivity = value; }
        }

        public PZ70DialPosition PZ70_DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public override String SettingsVersion()
        {
            return "2X";
        }

        public PZ70LCDButtonByteList LCDButtonByteListHandler => _lcdButtonByteListHandler;
    }


}
