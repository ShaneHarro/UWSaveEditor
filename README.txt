A save game editor for Ultima Underworld.

This is still heavily under construction, and using this program may damage or
corrupt your save files. The teleport feature only currently works with certain
characters. Please create a back up of your PLAYER.DAT file before using!

The teleportation utility is still extremely broken!

I will not be responsible for loss of player data!

USE AT YOUR OWN RISK!

Some notes:

*****************How the values are stored*****************

The developers over at Origin got a little tricky with how the player data is 
stored in the game. The starting byte in the PLAYER.DAT file is used as our
"Starting XOR value". To get the actual "decoded" value of the players stats,
Each byte starting at offset 0x01 has to be XOR'd with our (Starting XOR VALUE + increment)
where "increment" has an ititial value of 3 and increments by 3 for each byte.

Example: Let's say we had a file that (for the first few bytes) looked like this:

F9  AF  97  63

"F9" would be our starting XOR value. "AF" would then be XOR'd with F9 + 3 (which
is FC)

AF XOR FC = 53  (53 is our decoded value!)

Then for the next byte along "97" it's:

97 XOR (F9 + 6) = 97 XOR FF = 68  (68 is our next decoded value!)


But what happens when our starting XOR VALUE + increment goes over FF? The values
do a sort of "over flow" and the incrementation value is set to equal 0 (but still
adds 3 each byte).

Some code follows:



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



As you can see, it basically "peels" the 1 off the front of the hex value and
that's now our new Starting XOR value to work off for the rest of the file.



******************CHANGING STATS********************

All stats (starting with Strength) start at offset 0x1F and end with offset 0x35 
(Swimming - This utility does not yet edit Vitality or Mana pools.)

The values are read from the previously explained XOR "decryption". Once the actual
in game value is decoded and read, it's HEX equivalent value can be XOR'D with
the byte found in the stats offset to find a "term". Using this term, we can
edit our stat as required.

Example: Let's say our in-game strength is currently 21 and we want a strength 
level of 50.

First convert our 21 strength decimal into hexadecimal (which is 15)

Now we XOR 15 with the byte found at the strength offset (currently 3D)

15 XOR 3D = 28

28 is our term we can now use to edit our values to whatever we want!

Simply get the hex equivalent of the strength you want (We want 50, so the hex
equivilant is 32) and XOR it with our term

32 XOR 28 = 1A

We can now write 1A to the offset containing the strength value and it will be
set to 50! We can continue to do this for each stat as required!



******************CHANGING COORDINATES********************

Please remember that this program has an EXTREMELY broken teleportation tool and
I would advise against using it until further development has been done on it.

All coordinates are stored as int16's starting at offset 0x55. They seem to
be stored as little endian (that is, the first byte out of the two seems to
do minor jumps when edited, where as the second byte seems to do major jumps.)


    0x55 and 0x56 = X coordinates
    0x57 and 0x58 = Y coordinates
    0x59 and 0x5A = Z coordinates
    0x5B and 0x5C = Heading/Rotation


***********************************************************


Many thanks to Phat Tran who's documentation helped write this program:
ftp://files.chatnfiles.com/Infomagic-Windows/Infomagic-Games-for-Daze-Summer-95-1/EDITORS/UWEDITOR/UWEDITOR.TXT

And a HUGE thank you to this documentation which helped me figure out the XOR "encryption".
http://bootstrike.com/Ultima/Online/uwformat.php
