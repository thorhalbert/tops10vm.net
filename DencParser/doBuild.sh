echo "Build Lexer"
Gplex.exe Tokenizer.gplex 
echo "Build Parser"
Gppg.exe /gplex /nolines Parser.gppg
