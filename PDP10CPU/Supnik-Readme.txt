The CPU modules are ports of Robert Supnik's fine CPU modules for the simh software.
Their original headers/copyrights are present at the top of the files.

I thought very hard about writing this in C, in fact I did get quite a ways towards
that and had written a pretty complete CPU already (no floating point yet).   When
the simh for the pdp-10 appeared, my effort to emulate the original hardware became
obsolete (since the simh and the klh-10 emulators did such a good job).

So, I thought of an idea where I only really needed to only the user-mode elements
of the CPU (mostly).

I decided to start doing actual OS emulation so that one could run programs at
the command line (from unix or cygwin on windows).   I started with TOPS-10, since I
had programmed a KA-10 (I think they didn't get past the early TOPS-10 6 (I don't recall
the last kernel to support the KA).  I programmed TOPS-10 assembler for about 3 years
and TOPS-20 for only about 6 months.  I thought of just emulating TOPS-20 and permit
the PA1060 module to run to emulate TOPS-10, but I think I will simply emulate each.

I soon discovered, that in the limited time I had, the encode/decode logic to 
marshall the 36 bit call arguments were very tedious and this became pretty 
intolerable with the time constraints that I had, so eventually abandoned the 
project.

So, the new idea is to make this as object oriented as possible, and to use C# --and
to use a proven CPU (simh).   I have resurected the projects.  I have an idea for 
building a meta-code tool, which I have gotten pretty good with to automatically 
generate encode/decode classes for any OS (or hardware) entity.  I may even start
to push this into the CPU code to make it simpler.   C# execution may be slower than
C (likely to be, but probably not horribly so).   I may also replace the giant case
statement in the CPU with a large array of delegates (which is how I wrote the code
in C) -- I don't know how this might improve performance.  I am also heavily 
refactoring code to make it simpler.

And, of course, it would operate on real files and devices in the host system, 
instead of hosting them in a virtual machine.  So, this would operate not unlike
a java or c# virtual machine (ironically nested inside of a C# VM).



---Thor - 6/16/08
