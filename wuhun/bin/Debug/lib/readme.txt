TesseractEngine3.dll
tesseract-ocr .net for tesseract 3.01 release r638 

DLL)
	VC100 & .Net 4.0 Client Profile

Data)
	downlaod tessdata:
	https://code.google.com/p/tesseract-ocr/downloads/detail?name=tesseract-ocr-setup-3.01-1.exe

Sample)
	using OCR.TesseractWrapper;
	
	// init
	var processor = new TesseractProcessor();
	bool inited = processor.Init(); -- or -- processor.Init(@".\", "eng", 3);
	
	// load img
	Image image = Image.FromFile(filename);
	
	// recognize
	processor.Clear();
        processor.ClearAdaptiveClassifier();

	string result = processor.Recognize(image);

Site)
	https://code.google.com/p/tesseract-ocr
	https://code.google.com/p/tesseractdotnet/