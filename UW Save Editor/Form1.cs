//******************************************************************************
//Ultima Underworld Save Editor - Shane Harrison
//Documentation used:
//   ftp://files.chatnfiles.com/Infomagic-Windows/Infomagic-Games-for-Daze-Summer-95-1/EDITORS/UWEDITOR/UWEDITOR.TXT
//   http://bootstrike.com/Ultima/Online/uwformat.php
//
//******************************************************************************

//Some notes
//Address 0x3B seems to store the value which determines the avatars fatigue (awake, drowsy, fatigue, etc) 0x00 - 0x79 = "fatigued" ::::  0x80 - 0x9F = "awake" :::: 0xA0 - 0xAF "wide awake" :::: 0xB0 - 0xB7 "rested" :::: 0xB8 - 0xBF "wide awake" :::: 0xC0 - 0xCF "very tired" :::: 0xD0 - 0xDF  "fatigued" :::: 0xE0 - 0xFF "drowsy"
//Addresses 0x4B - 0x4E (4 bytes, 32 bit int?) seem to change values of max weight the avatar can carry. (maybe not 0x4E....test)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UW_Save_Editor
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        FileStream fs;
        bool fileOK = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void changeStatsButton_Click(object sender, EventArgs e)
        {
            //Grab all the values from our user inputs
            int strength = int.Parse(strBox.Text);
            int dexterity = int.Parse(dexBox.Text);
            int intelligence = int.Parse(intBox.Text);
            int attack = int.Parse(atkBox.Text);
            int defense = int.Parse(defBox.Text);
            int unarmed = int.Parse(unarmedBox.Text);
            int sword = int.Parse(swrdBox.Text);
            int axe = int.Parse(axeBox.Text);
            int mace = int.Parse(maceBox.Text);
            int missile = int.Parse(mslBox.Text);
            int mana = int.Parse(manaStatBox.Text);
            int lore = int.Parse(loreBox.Text);
            int casting = int.Parse(castBox.Text);
            int traps = int.Parse(trapBox.Text);
            int search = int.Parse(searchBox.Text);
            int track = int.Parse(trackBox.Text);
            int sneak = int.Parse(sneakBox.Text);
            int repair = int.Parse(repairBox.Text);
            int charm = int.Parse(charmBox.Text);
            int picklock = int.Parse(pickBox.Text);
            int acrobat = int.Parse(acroBox.Text);
            int appraise = int.Parse(appBox.Text);
            int swimming = int.Parse(swimBox.Text);


            int startXOR;
            int xorIncrement = 0x03;
            const int NAME_LENGTH = 15;
            const int NO_OF_BYTES = 53;
            char nameCharacter;
            string nameString = "";
            byte byteConvered;
            int statReq;


            //Array that holds all the user input fields
            int[] stats = {strength, dexterity, intelligence, attack, defense, unarmed, sword, axe, mace, missile, mana, lore, casting, traps, search, track, sneak, repair,
                           charm, picklock, acrobat, appraise, swimming};


            //Encoded bytes read before XOR calculations
            int[] encodedBytes = new int[NO_OF_BYTES];

            //Will store our values after undoing the XOR "encryption"
            int[] decodedValues = new int[NO_OF_BYTES];

            //All the XOR terms for each byte
            int[] XORterms = new int[NO_OF_BYTES];

            //If file open and correct format
            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);

                fs.Position = 0x00;

                //Will read offset 0x00 (this will be our starting XOR value)
                startXOR = fs.ReadByte();

               // fs.Position = 0x01;


                //Read all the bytes between 0x01 (The first character of name) and 0x35 (Value which determines swimming)
                for (int i = 0; i < encodedBytes.Length; i++)
                {
                    encodedBytes[i] = fs.ReadByte();
                }

                for (int i = 0; i < decodedValues.Length; i++)
                {
                    decodedValues[i] = encodedBytes[i] ^ (startXOR + xorIncrement);
                    xorIncrement += 3;

                    //If the Starting XOR + our increment goes over 0xFF, check its value and set our values back accordingly
                    if (xorIncrement + startXOR > 0xFF)
                    {
                        MessageBox.Show("Overflow warning: " + (xorIncrement + startXOR).ToString("x"));

                        if (xorIncrement + startXOR == 0x100)
                        {
                            startXOR = 0x00;
                        }

                        if (xorIncrement + startXOR == 0x101)
                        {
                            startXOR = 0x01;
                        }

                        if (xorIncrement + startXOR == 0x102)
                        {
                            startXOR = 0x02;
                        }

                        xorIncrement = 0;
                    }
                }

                //Get players name and store it as a string in "nameString"
                for (int i = 0; i < NAME_LENGTH; i++)
                {
                    nameCharacter = (char)decodedValues[i];
                    nameString += nameCharacter.ToString();
                }

                //Show characters name on UI
                playerNameLabel.Text += nameString;

                //Calculate all our XOR terms 
                for (int i = 0; i < XORterms.Length; i++)
                {
                    XORterms[i] = decodedValues[i] ^ encodedBytes[i];
                }


                //Set our position to strength byte
                fs.Position = 0x1F;

                //Loop through all bytes between strength and swimming, calculate and set value accordingly
                for (int i = 0; i < stats.Length; i++)
                {
                    //i + 30 because strength starts at offset 31 (relative to length between offset 0x01 and 0x1F) and arrays start at [0]
                    statReq = stats[i] ^ XORterms[i + 30];
                    byteConvered = Convert.ToByte(statReq);
                    fs.WriteByte(byteConvered);
                }

                fs.Close();

                MessageBox.Show("Stats updated!");

            }
        }


                /*


                        //Will read offset 0x36 after previous edits
                        strengthByte = fs.ReadByte();

                //Calculate term
                codeTerm = calcTerm(int.Parse(StrengthSync.Text), strengthByte);

                //Loop for as many as there are stats
                for (int i = 0; i < stats.Length; i++ )
                {
                    //If we are on the first loop, set our position offset back to the byte that holds Strength
                    if (i == 0)
                    {
                         fs.Position = 0x1F;
                    }

                    //Calculate the byte change required to change to requested value
                    changeByte = calcChange(stats[i], codeTerm);

                    //Convert "ChangeByte" string returned from function into a byte
                    changeByteConvert = byte.Parse(changeByte);

                    //Write our new stat calculation
                    fs.WriteByte(changeByteConvert);

                    //Converts term hex string into a usable hex value
                    int termTemp = int.Parse(codeTerm, NumberStyles.HexNumber);
                  
                    //Add 3 to term
                    termTemp += 0x03;

                    //Check code terms to make sure they're not incrementing past 0xFF
                    if (termTemp == 0xFF)
                    {
                       termTemp = 0x02;
                    }

                    //Check code terms to make sure they're not incrementing past 0xFF
                    if (termTemp == 0xFE)
                    {
                        termTemp = 0x01;
                    }

                    //Check code terms to make sure they're not incrementing past 0xFF
                    if (termTemp == 0xFD)
                    {
                        termTemp = 0x00;
                    }


                    //Store our term + 3 back into term string for next loop
                    codeTerm = termTemp.ToString("X");
                }
                                         
                fs.Close();

                //Update new "strength sync" with last stat change
                StrengthSync.Text = strBox.Text;

                MessageBox.Show("Stats updated!");
            }

            else
            {
                MessageBox.Show("No file selected.");
            }           
        }

        //Calculate term based on strength
        string calcTerm(int strengthValue, int StrengthByte)
        {
            string hexTerm;
            int XORvalue;
            XORvalue = strengthValue ^ StrengthByte;
            hexTerm = XORvalue.ToString("X");

            return hexTerm;
        }

        //Calculate term based on strength
        string calcChange(int stat, string termString)
        {
            int XORchange;
            int termValue = int.Parse(termString, NumberStyles.AllowHexSpecifier);
            XORchange = stat ^ termValue;
            string changeByte = XORchange.ToString();

            return changeByte;
        }

*/
        private void openFileButton_Click(object sender, EventArgs e)
        {       
            if (ofd.ShowDialog() == DialogResult.OK);
            {
                //Check if correct file extension 
                string filePath = ofd.FileName;
                string fileName = Path.GetFileName(filePath);
                string fileExtension = fileName.GetLast(4);

                if (fileExtension != ".DAT")
                {
                    fileOK = false;
                    MessageBox.Show("Incorrect file type.");
                }

                else
                {
                    fileOK = true;
                }
            }
        }


        //Teleportation still under construction
        private void teleportButton_Click(object sender, EventArgs e)
        {
          
            if (fileOK == true)
            {

                fs = new FileStream(ofd.FileName, FileMode.Open);

                // EA 14 CA 13 14 14 - Start coordinate
                if (startRadio.Checked)
                {
                    fs.Position = 0x55;

                    //x (or y?)
                    fs.WriteByte(0xEA);
                    fs.WriteByte(0x14);

                    //y (or x?)
                    fs.WriteByte(0xCA);
                    fs.WriteByte(0x13);

                    //z
                    fs.WriteByte(0x14);
                    fs.WriteByte(0x14);
                }


                //CA 27 81 22 15 14 - Gray Goblins
                if (grayRadio.Checked)
                {
                    fs.Position = 0x55;

                    //x (or y?)
                    fs.WriteByte(0xCA);
                    fs.WriteByte(0x27);

                    //y (or x?)
                    fs.WriteByte(0x81);
                    fs.WriteByte(0x22);

                    //z
                    fs.WriteByte(0x15);
                    fs.WriteByte(0x14);
                }

                //64 14 93 27 13 14 - Spider Cave
                if (spiderRadio.Checked)
                {
                    fs.Position = 0x55;

                    //x (or y?)
                    fs.WriteByte(0x64);
                    fs.WriteByte(0x14);

                    //y (or x?)
                    fs.WriteByte(0x93);
                    fs.WriteByte(0x27);

                    //z
                    fs.WriteByte(0x13);
                    fs.WriteByte(0x14);
                }

               
               

                MessageBox.Show("Coordinates updated!");
                fs.Close();
            }
        }
    }


    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
