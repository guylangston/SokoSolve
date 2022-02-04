# Simple Text/String Helpers

- Humanise
- FluentString
- StringUtil
```
  string UnCamel(string)
  string ParseAndReplaceVariables(string, Func<string,string>)
  string StripLineFeeds(string)
  IEnumerable<string> ToLines(string)
  string Repeat(string, Int32)
  string Truncate(string, Int32, string)
  string PadCentre(Int32, string)
  string TakeMax(string, Int32)
  string TextBetween(string, string, string)
  string TextBetween(string, Int32, Int32, Int32)
  string Elipse(string, Int32, string)
  (string,string) SplitAtNotInclusive(string, Int32)
  (string,string) SplitAtInclusive(string, Int32)
  string TrimWhile(string, Func`2[Char,Boolean])
 ```
## Basic Report Builder

`MapToReporting<SomeObj>`
