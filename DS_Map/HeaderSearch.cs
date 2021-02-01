﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DSPRE {
    public partial class HeaderSearch : Form {
        private List<string> searchableHeaderFieldsList = new List<string>() {
            "Area Data (ID)",
            "Camera Angle (ID)",
            "Event File (ID)",
            "Internal Name",
            "Level Script (ID)",
            "Matrix (ID)",
            "Music Day (ID)",
            //Maybe in the future - "Music Day (Name)",
            "Music Night (ID)",
            //Maybe in the future - "Music Night (Name)",
            "Script File (ID)",
            "Text Archive (ID)",
            "Weather (ID)",
        };

        private List<string> headerSearchNumericOperatorsList = new List<string>() {
            "Equals",
            "Is Different than",
            "Is Less than",
            "Is Less than or Equal to",
            "Is Greater than",
            "Is Greater than or Equal to",
        };
        private List<string> headerSearchTextOperatorsList = new List<string>() {
            "Is Exactly",
            "Is Not",
            "Contains",
            "Does not contain"
        };

        private string[] searchableHeaderFields;
        private string[] headerSearchNumericOperators;
        private string[] headerSearchTextOperators;

        private List<string> intNames;
        private ListBox headerListBox;

        public string status = "Ready";

        public HeaderSearch(ref List<string> internalNames, ListBox headerListBox) {
            InitializeComponent();
            searchableHeaderFields = searchableHeaderFieldsList.ToArray();
            headerSearchNumericOperators = headerSearchNumericOperatorsList.ToArray();
            headerSearchTextOperators = headerSearchTextOperatorsList.ToArray();

            intNames = internalNames;
            this.headerListBox = headerListBox;

            fieldToSearch1ComboBox.Items.AddRange(searchableHeaderFields);
            fieldToSearch1ComboBox.SelectedIndex = 0;
            operator1ComboBox.SelectedIndex = 0;
        }

        private void fieldToSearch1ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateOperators(operator1ComboBox, fieldToSearch1ComboBox);
        }

        #region Helper Methods
        private void UpdateOperators(ComboBox operatorComboBox, ComboBox fieldToSearchComboBox) {
            operatorComboBox.Items.Clear();

            if (fieldToSearchComboBox.SelectedItem.ToString().Contains("ID")) {
                operatorComboBox.Items.AddRange(headerSearchNumericOperators);
            } else {
                operatorComboBox.Items.AddRange(headerSearchTextOperators);
            }

            operatorComboBox.SelectedIndex = 0;
        }
        #endregion
        public static List<string> advancedSearch(short startID, short finalID, List<string> intNames, string fieldToSearch, string oper, string valToSearch) {
            if (fieldToSearch == "" || oper == "" || valToSearch == "")
                return null;

            List<string> result = new List<string>();

            switch (fieldToSearch) {
                case "Internal Name":
                    for (short i = startID; i < finalID; i++) {
                        if (oper.Equals("Is Exactly"))
                            if (intNames[i].Equals(valToSearch)) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        if (oper.Equals("Is Not"))
                            if (!intNames[i].Equals(valToSearch)) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        if (oper.Equals("Contains"))
                            if (intNames[i].Contains(valToSearch)) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        if (oper.Equals("Does not contain"))
                            if (!intNames[i].Contains(valToSearch)) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                    }
                    break;
                case "Music Day (Name)":
                    //Maybe in the future
                    break;
                case "Music Night (Name)":
                    //Maybe in the future
                    break;
                default:
                    string[] fieldSplit = fieldToSearch.Split();
                    fieldSplit[0] = fieldSplit[0].ToLower();
                    fieldSplit[fieldSplit.Length - 1] = fieldSplit[fieldSplit.Length - 1].Replace("(", ""); //Remove ( from string
                    fieldSplit[fieldSplit.Length - 1] = fieldSplit[fieldSplit.Length - 1].Replace(")", ""); //Remove ) from string

                    string property = String.Join("", fieldSplit);
                    int numToSearch = int.Parse(valToSearch);

                    for (short i = startID; i < finalID; i++) {
                        var headerFieldEntry = typeof(Header).GetProperty(property).GetValue(Header.LoadFromARM9(i), null);
                        int headerField = int.Parse(headerFieldEntry.ToString());

                        if (oper.Equals("Is Less than")) {
                            if (headerField < numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        } else if (oper.Equals("Equals")) {
                            if (headerField == numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        } else if (oper.Equals("Is Greater")) {
                            if (headerField > numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        } else if (oper.StartsWith("Is Less than or Equal")) {
                            if (headerField <= numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        } else if (oper.StartsWith("Is Greater than or Equal")) {
                            if (headerField >= numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        } else if (oper.StartsWith("Is Different")) {
                            if (headerField != numToSearch) {
                                result.Add(i.ToString("D3") + Header.nameSeparator + intNames[i]);
                            }
                        }
                    }
                    break;
            }
            return result;
        }
        private void startSearchButton_Click(object sender, EventArgs e) {
            headerListBox.Items.Clear();
            List<string> result;

            if (value1TextBox.Text == "") {
                MessageBox.Show("Value to search is empty", "Can't search", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try {
                result = advancedSearch(0, (short)intNames.Count, intNames, fieldToSearch1ComboBox.Text, operator1ComboBox.SelectedItem.ToString(), value1TextBox.Text);
            } catch (FormatException) {
                MessageBox.Show("Make sure the value to search is correct.", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                headerListBox.Items.Add("Search parameters are invalid");
                headerListBox.Enabled = false;
                return;
            }

            string searchConfiguration = fieldToSearch1ComboBox.Text + " " + operator1ComboBox.Text.ToLower() + " " + '"' + value1TextBox.Text + '"';
            if (result.Count <= 0) {
                headerListBox.Items.Add("No header's " + searchConfiguration);
                headerListBox.Enabled = false;
            } else {
                headerListBox.Items.AddRange(result.ToArray());
                headerListBox.SelectedIndex = 0;
                headerListBox.Enabled = true;

                status = "Showing headers whose " + searchConfiguration;
            }
        }
    }
}