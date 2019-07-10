# GoogleTranslateFreeApi
Api for free text translation using Google translate.

| 	                   |  	Badge		|
| -------------------------|:------------------:|
| **Target Framework**     | [![.Net Standard 1.1](https://img.shields.io/badge/.NET%20Standard-1.1-green.svg)](https://docs.microsoft.com/ru-ru/dotnet/standard/net-standard) |
| **Nuget**		   | [![Nuget](https://img.shields.io/nuget/v/GoogleTranslateFreeApi.svg)](https://www.nuget.org/packages/GoogleTranslateFreeApi/)
| **License** 		   | [![MIT](https://img.shields.io/github/license/Grizley56/GoogleTranslateFreeApi.svg)](https://opensource.org/licenses/MIT) |

Main features:
- [x] Text corrections
- [x] Language corrections
- [x] Language auto detection
- [x] Transcriptions (original text, translated text)
- [x] Synonyms
- [x] Definitions + Examples
- [x] Extra translations
- [x] Proxy (Useful when getting a ban for a while)

---
| Feature                  | TranslateLiteAsync | TranslateAsync  |
| -------------------------|:------------------:|:---------------:|
| Text corrections         |         +          |         +       |
| Language corrections     |         +          |         +       |
| Language auto detection  |         +          |         +       |
| Transcriptions           |         +          |         +       |
| Synonyms                 |         -          |         +       |
| Definitions + Examples   |         -          |         +       |
| Extra translations       |         -          |         +       |
| See Also                 |         -          |         +       |
---

# <p align="center"> Usage </p>

<p align="center"><b>Translation, transcription and text/language corrections</b></p>

```C#

var translator = new GoogleTranslator();

Language from = Language.Auto;
Language to = GoogleTranslator.GetLanguageByName("Japanese");

TranslationResult result = await translator.TranslateLiteAsync("Hello. How are you?", from, to);

//The result is separated by the suggestions and the '\n' symbols
string[] resultSeparated = result.FragmentedTranslation;

//You can get all text using MergedTranslation property
string resultMerged = result.MergedTranslation;

//There is also original text transcription
string transcription = result.TranslatedTextTranscription; // Kon'nichiwa! Ogenkidesuka?

```
---
<p align="center"><b>Language auto detection and correction</b></p>


```C#
var result2 = await translator.TranslateLiteAsync("Drones", Language.Auto, Language.Czech);

foreach(LanguageDetection languageDetection in result2.LanguageDetections)
{
	Console.WriteLine(languageDetection);
}

// The output will be next. Double in brackets is confidence
// Spanish (0.61010)
// English (0.34365)

Language spanish = new Language("Spanish", "es"); // For the method, only the second parameter is important (ISO639)

result2 = await translator.TranslateLiteAsync("world", spanish, Language.Czech);
Console.WriteLine(result2.LanguageDetections.First()); // English (0.98828)
Console.WriteLine(result2.MergedTranslation) // мир
```
---
<p align="center"><b>Text corrections</b></p>

```C#
string misspellingsText = "The quik brown fox jumps ovver the lazy dog"

var english = new Language("English language", "en");
// you could also get language from Language static properties
// for example: Language.English or Language.Korean

// check language is valid for GoogleTranslator class
Console.WriteLine(GoogleTranslator.IsLanguageSupported(english)); 

var result3 = await translator.TranslateLiteAsync(misspellingsText, english, GoogleTranslator.GetLanguageByISO("ru"));

if(result3.Corrections.TextWasCorrected)
	Console.WriteLine(string.Join(",", result3.Corrections.CorrectedWords); // "quick", "over"
	
Console.WriteLine(result3.MergedTranslation) // "Быстрая коричневая лиса прыгает через ленивую собаку"

```
---
<p align="center"><b>Synonyms, extra translations, definitions and so on. </b> </p>

```C#

GoogleTranslator translator = new GoogleTranslator();

var result4 = await translator.TranslateAsync("книга", Language.Russian, Language.English);

if(result.ExtraTranslations != null)
  Console.WriteLine(result.ExtraTranslations.ToString()); // ToString returns friendly for reading string
  
```
_Example of ToString output:_ 

```
Noun:
book: книга, книжка, журнал, книжечка, том, текст
volume: объем, том, громкость, книга, емкость, масса
Abbreviation:
bk: книга, назад, обратно
```
---
<p align="center"><b>You can also get any part of the speech indently</b></p>

```C#
foreach(ExtraTranslation item in result4.ExtraTranslations)
  Console.WriteLine($"{item.Phrase}: {String.Join(", ", item.PhraseTranslations)}"); 
  // just like item.ToString()
```
### There are other translate information getting by the same principe
- Definitions
- Synonyms
- See also

---
Google Translate can ban IP that sends too many requests at the same time. Ban lasts about a few hours, but you can use a Proxy
```C#
var proxy = new WebProxy(uri); // You also can use GoogleTranslateFreeApi.Proxy class for this
translator.Proxy = proxy;
```

## LICENSE

Released under the [MIT](https://opensource.org/licenses/MIT) License.

This repository сontains part of the code from the library [google-translate-token](https://github.com/matheuss/google-translate-token)
