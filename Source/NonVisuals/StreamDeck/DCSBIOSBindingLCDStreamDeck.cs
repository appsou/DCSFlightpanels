﻿using System;
using DCS_BIOS;

namespace NonVisuals
{

    public class DCSBIOSBindingLCDStreamDeck
    {
        /*
         * This class binds a DCSBIOSOutput with an LCD (key) on a Stream Deck
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDStreamDeck, all data will be shown
         */
        private StreamDeckButtons _streamDeckButton;
        private int _currentValue = 0;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private const string SeparatorChars = "\\o/";
        private string _layer = "";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("StreamDeckDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                //StreamDeckDCSBIOSControlLCD{Home Layer|Button1}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSControlLCD{Home Layer|Button1}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSControlLCD{", "").Replace("}", "");
                //Home Layer|Button1
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];
                StreamDeckButton = (StreamDeckButtons)Enum.Parse(typeof(StreamDeckButtons), param0Split[1]);

                //[1]
                //DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[1]);
            }
            if (settings.StartsWith("StreamDeckDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //StreamDeckDCSBIOSControlLCD{Home Layer|Button1}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //StreamDeckDCSBIOSControlLCD{Home Layer|Button1}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSControlLCD{", "").Replace("}", "");
                //Home Layer|Button1
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                Layer = param0Split[0];
                StreamDeckButton = (StreamDeckButtons)Enum.Parse(typeof(StreamDeckButtons), param0Split[1]);

                //[1]
                //DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[1]);
            }
        }

        public string Layer
        {
            get => _layer;
            set => _layer = value;
        }

        public StreamDeckButtons StreamDeckButton
        {
            get => _streamDeckButton;
            set => _streamDeckButton = value;
        }
        
        public int CurrentValue
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        public DCSBIOSOutput DCSBIOSOutputObject
        {
            get => _dcsbiosOutput;
            set
            {
                _dcsbiosOutput = value;
                _dcsbiosOutputFormula = null;
            }
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormulaObject
        {
            get => _dcsbiosOutputFormula;
            set
            {
                _dcsbiosOutputFormula = value;
                _dcsbiosOutput = null;
            }
        }


        public string ExportSettings()
        {
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }
            if (_dcsbiosOutputFormula != null)
            {
                //StreamDeckDCSBIOSControlLCD{Home Layer|ALT}\o/{Button11Left}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "StreamDeckDCSBIOSControlLCD{" + Layer + "|" + Enum.GetName(typeof(StreamDeckButtons), _streamDeckButton) + "}" + SeparatorChars + _dcsbiosOutputFormula.ToString();
            }
            return "StreamDeckDCSBIOSControlLCD{" + Layer + "|" + Enum.GetName(typeof(StreamDeckButtons), _streamDeckButton) + "}" + SeparatorChars + _dcsbiosOutput.ToString();
        }
        
        public bool HasBinding => _dcsbiosOutput != null || _dcsbiosOutputFormula != null;

        public bool UseFormula => _dcsbiosOutputFormula != null;
    }
}