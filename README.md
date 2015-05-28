# UWSaveEditor
A save game editor for ultima Underworld.

This is still heavily under construction, and using this program may damage or corrupt your save files. 

USE AT YOUR OWN RISK!


Some notes:

All stats (starting with Strength) start at offset 0x1F (At least with my current tests...This might differ depending on the particular save file!). This utility does not edit Vitality and Mana (pool, not stat) as I have yet to discover the relationship between the bytes stored in their offsets and the their actual in game values.

The program requires you to input your current in-game strength to calculate the "Code Term". The code term created depends on your name, so a single stat (strength in this case) is required to "work backwards" to figure it out. 

If your in-game strength is 50 (32 HEX) and the current byte at offset 0x1F is 64 HEX, then you <strong>XOR</strong> these two values together to calculate your code term.

0x32 <strong>XOR</strong> 0x64  = 0x56

0x56 is your code term. 

Using this code term, we can now figure out which byte we need to put in 0x1F to change our strength. If we wanted to change our strength to 60 (3C HEX), we just <strong>XOR</strong> our code term with desired level (in hex).

0x3C <strong>XOR</strong> 0x56 = 0x6A

We can now place 0x6A in offset 0x1F to make our strength 60!

For each stat after strength, the "Code Term" is incremented by 3, so for "Dexterity" (The next stat after strength) the code term in this example would be 0x59.

If the code term hits 0xFF (255, max size of a byte) then it tries to increment another 3, it "rolls over" back to 0x02.

Please remember that all Code Terms are different for each character and I have not yet figured out a way to automatically calculate the term without having the user enter their strength level first! Looking forward to hearing from someone who has this figured out!






Many thanks to Phat Tran who's documentation helped write this program:
ftp://files.chatnfiles.com/Infomagic-Windows/Infomagic-Games-for-Daze-Summer-95-1/EDITORS/UWEDITOR/UWEDITOR.TXT
