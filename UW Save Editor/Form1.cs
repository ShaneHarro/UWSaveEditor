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



        const byte strengthOffset = 0x1F;
        const int NAME_LENGTH = 15;
        const int NO_OF_BYTES = 200; //From 0x01 to 0x5D (where 0x5D stores current floor/level)
        const int NO_OF_STATS = 23;
        int startXOR;
        int xorIncrement = 0x03;
        int statLevelWanted;
        char nameCharacter;
        string nameString = "";
        byte byteConvered;


        //Encoded bytes read before XOR calculations
        int[] encodedBytes = new int[NO_OF_BYTES];

        //Will store our values after undoing the XOR "encryption"
        int[] decodedValues = new int[NO_OF_BYTES];

        //All the XOR terms for each byte
        int[] XORterms = new int[NO_OF_BYTES];

        public Form1()
        {
            InitializeComponent();
        }



        private void changeStatsButton_Click(object sender, EventArgs e)
        {

            TextBox[] textBoxArray = {strBox, dexBox, intBox, atkBox, defBox, unarmedBox, swrdBox, axeBox, maceBox, mslBox, manaStatBox,
                                      loreBox, castBox, trapBox, searchBox, trackBox, sneakBox, repairBox, charmBox, pickBox, acroBox,
                                      appBox, swimBox};

            //If file open and correct format
            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);

                //Set our position to strength byte offset (starting offset for all stats)
                fs.Position = strengthOffset;

                //Loop through all bytes between strength and swimming, calculate and set value accordingly
                for (int i = 0; i < textBoxArray.Length; i++)
                {
                    //i + 30 because strength starts at offset 31 (relative to length between offset 0x01 and 0x1F) and arrays start at [0]
                    statLevelWanted = int.Parse(textBoxArray[i].Text) ^ XORterms[i + 30];
                    byteConvered = Convert.ToByte(statLevelWanted);
                    fs.WriteByte(byteConvered);
                }

                fs.Close();

                MessageBox.Show("Stats updated!");
            }

            else
            {
                MessageBox.Show("Please select a valid file.");
            }
        }


        private void openFileButton_Click(object sender, EventArgs e)
        {

            if (ofd.ShowDialog() == DialogResult.OK)
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
                    fs = new FileStream(ofd.FileName, FileMode.Open);
                    
                    TextBox[] textBoxArray = {strBox, dexBox, intBox, atkBox, defBox, unarmedBox, swrdBox, axeBox, maceBox, mslBox, manaStatBox,
                                              loreBox, castBox, trapBox, searchBox, trackBox, sneakBox, repairBox, charmBox, pickBox, acroBox,
                                              appBox, swimBox}; 

                    fs.Position = 0x00;

                    //Will read offset 0x00 (this will be our starting XOR value)
                    startXOR = fs.ReadByte();

                    MessageBox.Show(startXOR.ToString("x"));

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


                    //Calculate all our XOR terms 
                    for (int i = 0; i < XORterms.Length; i++)
                    {
                        XORterms[i] = decodedValues[i] ^ encodedBytes[i];
                    }


                    for (int i = 0; i < textBoxArray.Length; i++)
                    {
                        textBoxArray[i].Text = (decodedValues[i + 30]).ToString();
                    }

                     //Get players name and store it as a string in "nameString"
                    for (int i = 0; i < NAME_LENGTH; i++)
                    {
                        nameCharacter = (char)decodedValues[i];
                        nameString += nameCharacter.ToString();
                    }

                    //Show characters name on UI
                    playerNameLabel.Text = "Player name: " + nameString;


                    fs.Close();
                }
            }
        }

        //Teleportation still under construction
        private void teleportButton_Click(object sender, EventArgs e)
        {
            fs = new FileStream(ofd.FileName, FileMode.Open);


            const byte positionOffset = 0x55;
            byte xPos_1 = 0;
            byte xPos_2 = 0;
            byte yPos_1 = 0;
            byte yPos_2 = 0;
            byte zPos_1 = 0;
            byte zPos_2 = 0;

            if (fileOK == true)
            {
                fs.Position = positionOffset;

                if (startRadio.Checked)
                {
                    //Start coordinates = X = 208.200    Y = 18.65  Z = 19.16 (Keep in mind: little endian!)
                    xPos_1 = Convert.ToByte(200 ^ XORterms[84]);
                    xPos_2 = Convert.ToByte(208 ^ XORterms[85]);

                    yPos_1 = Convert.ToByte(65 ^ XORterms[86]);
                    yPos_2 = Convert.ToByte(18 ^ XORterms[87]);

                    zPos_1 = Convert.ToByte(16 ^ XORterms[88]);
                    zPos_2 = Convert.ToByte(19 ^ XORterms[89]);
                }

                if (grayRadio.Checked)
                {
                    //Gray Goblins coordinates = X = 220.175   Y = 35.67  Z = 19.18 (Keep in mind: little endian!)
                    xPos_1 = Convert.ToByte(175 ^ XORterms[84]);
                    xPos_2 = Convert.ToByte(220 ^ XORterms[85]);

                    yPos_1 = Convert.ToByte(67 ^ XORterms[86]);
                    yPos_2 = Convert.ToByte(35 ^ XORterms[87]);

                    zPos_1 = Convert.ToByte(18 ^ XORterms[88]);
                    zPos_2 = Convert.ToByte(19 ^ XORterms[89]);
                }


                fs.WriteByte(xPos_1);
                fs.WriteByte(xPos_2);

                fs.WriteByte(yPos_1);
                fs.WriteByte(yPos_2);

                fs.WriteByte(zPos_1);
                fs.WriteByte(zPos_2);

            }

            MessageBox.Show("Coordinates updated!");
            fs.Close();
        }




        private void coordsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("X position: " + decodedValues[85].ToString() + "." + decodedValues[84].ToString() + "\n" +
             "Y position: " + decodedValues[87].ToString() + "." + decodedValues[86].ToString() + "\n" +
             "Z position: " + decodedValues[89].ToString() + "." + decodedValues[88].ToString());
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
