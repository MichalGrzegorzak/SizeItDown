@ECHO ON
SET INPUT="v:\Conversion\TESTS"
SET OUT="v:\Conversion\TEMP"
SET VID_PRESET="HandbrakeVideoPreset.json"
SET IMG_QUALITY=80
::SET IMG_CROP_TO=9999
SET IMG_CROP_TO=2560
SET IMG_FORMAT=webp  
:: or Avif, but it seems Google doesn't fully supports this format yet

:: -t = testmode - try this first, it will output files into TEMP directory, where you can inspect them
:: -a = autoReplace - opposite to -t, fastest way to convert everything, as it deletes old files
:: -v = convert videos
:: -g = convert images
:: if you don't want to resize images just increase IMG_CROP_TO to 99999

:: 1) Test mode, safe mode, it will not replace any original file
SizeItDown.exe -v -g -i %INPUT% --tempOutputDir %OUT% -p %VID_PRESET% -q %IMG_QUALITY% -w %IMG_CROP_TO% -c %IMG_FORMAT% -t

::-----------------------------------------------
:: 2) PART 1/2 - Conversion mode, same as before just removed -t
::SizeItDown.exe -v -g -i %INPUT% --tempOutputDir %OUT% -p %VID_PRESET% -q %IMG_QUALITY% -w %IMG_CROP_TO% -c %IMG_FORMAT%

:: Here you can have some time to investigate and only if you are happy with conversion results, go to PART2

:: 3) PART 2/2 - Replace mode (-r was added) Replace your files in the input folder - these is it
::SizeItDown.exe -v -g -i %INPUT% --tempOutputDir %OUT% -p %VID_PRESET% -q %IMG_QUALITY% -w %IMG_CROP_TO% -c %IMG_FORMAT% -r
::-----------------------------------------------

:: 4) EXPERT mode (-a was added) Fastest way, it automatically replaces old files during processing - hope you know what you are doing
::SizeItDown.exe -v -g -i %INPUT% --tempOutputDir %OUT% -p %VID_PRESET% -q %IMG_QUALITY% -w %IMG_CROP_TO% -c %IMG_FORMAT% -a
