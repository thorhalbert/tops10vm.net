# tops10vm.net
TOPS-10 Emulator in .NET

This was my second attempt at writing a tops-10 (and possibily tops-20) emulator, but in C# instead of C.

The root of this was a port of the SIMH pdp-10 processor engine.  I make no claims on this excellent original code.  And give full credit to Bob Supnik and others involved with the project.
The SIMH version I branched from was rather old, but so then is the 10 code.

There's a graphical UI which shows the processor run, and there's an amusing set of modern unit tests that run 45 year old paper-tape images.

This has the beginnings of the DENC language which would build encoder/decoder logic.   However, there have been modern efforts that might work better (and are more completed), like https://github.com/kaitai-io.   If all of the 36 bit words were packed into 64 bit integers, then perhaps it would be easy enough.  I think Kaitai has bit-field type operations.   
