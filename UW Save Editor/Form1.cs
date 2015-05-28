//******************************************************************************
//Ultima Underworld Save Editor - Shane Harrison
//Documentation used: ftp://files.chatnfiles.com/Infomagic-Windows/Infomagic-Games-for-Daze-Summer-95-1/EDITORS/UWEDITOR/UWEDITOR.TXT
//                      http://bootstrike.com/Ultima/Online/uwformat.php
//
//******************************************************************************

/*Some notes
 * Address 0x3B seems to store the value which determines the avatars fatigue (awake, drowsy, fatigue, etc) 0x00 - 0x79 = "fatigued" ::::  0x80 - 0x9F = "awake" :::: 0xA0 - 0xAF "wide awake" :::: 0xB0 - 0xB7 "rested" :::: 0xB8 - 0xBF "wide awake" :::: 0xC0 - 0xCF "very tired" :::: 0xD0 - 0xDF  "fatigued" :::: 0xE0 - 0xFF "drowsy"
 *Addresses 0x4B - 0x4E (4 bytes, 32 bit int?) seem to change values of max weight the avatar can carry. (maybe not 0x4E....test)
 *
 *Address 0x55 to 0x58 seems to contain Coordinate location data for the player (but seems stored in reversed order..) It feels like 0x56 and 0x55 store data in reverse order, i.e 0x56 is the "whole value" and 0x55 is the "Decimal", e.g 23.43343 where "23" would be held in 0x56 and .43343 held in 0x55
 *
 * e.g coordinate = X.XXXXXX (EAST-WEST)      Y.YYYYYY (NORTH-SOUTH)
 *                  ^    ^                    ^    ^
 *                0x56  0x55                 0x58  0x57
 *                  
 * Z coordinate (up and down) is stored in 0x59 and 0x5A in the same reversed fashion
 * 
 * Rotation is stored in 0x5B and 0x5C in same reversed fashion
 */

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
            int strengthByte;
            string codeTerm;
            string changeByte;
            byte changeByteConvert;

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

            //Array that holds all the user input fields
            int[] stats = {strength, dexterity, intelligence, attack, defense, unarmed, sword, axe, mace, missile, mana, lore, casting, traps, search, track, sneak, repair,
                           charm, picklock, acrobat, appraise, swimming};

            //If file open
            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);

                //Move to offset for strength so we can read the starting byte
                fs.Position = 0x1F;

                //Read byte at strength offset and store it in strengthByte (NOTE: This will increment offset position! As does writing!)
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

                    //If code term goes above maximum byte size of 0xFF (255), Bring it back to 0x02 to prevent overflow
                    if (termTemp > 0xFF)
                    {
                       termTemp = 0x02;
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


        private void openFileButton_Click(object sender, EventArgs e)
        {       
            if (ofd.ShowDialog() == DialogResult.OK);
            {
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
          //Start coordinates: EA 14 CA 13 14 14
            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);

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
