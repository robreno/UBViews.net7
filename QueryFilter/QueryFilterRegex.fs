namespace QueryFilterLib

    module QueryFilterRegex =

        open System
        open System.IO
        open System.Text
        open System.Text.RegularExpressions

        // ----------------------------------------------------------------------------

        let StringSplitPatternArray = [|" "; "/"; "\\"; ","; "."; ";"; "?"; "!"; ":"; ")"; "("; "<"; ">"; "["; "]"; "}"; 
                                   "{"; "+"; (*"-";*) "*"; "%"; "_"; "#"; "&"; "§"; "$"; "~"; "|"; "^"; "\""; (*"'";*) 
                                   "="; "”"; "“"; (*"’"; "‘";*) "—"; |]

        let invalidRomanNumeralsReg = Regex( "(^i{1,3}|(^iv|^v|^vi{0,3})|(^ix|^x|^xi{1,2}))" )
        let invalidWordsReg = Regex("^\d+$|^\d+\w+$|^\w+\d+$|^\d+.+$|\s+|(�|©|@|®)+")
        let invalidAposReg = Regex("(^’|^‘|’$|‘$)")
        let invalidEndDashCharReg = Regex("(-$)")
        let invalidEndDashReg = Regex("(‑$|-$)")
        let isEmptyStringReg = Regex("^$")
        let validAposReg = Regex("(\’[a-z]{1,2}$)")
        let pidPattern = Regex("^\d+:\d+.\d+\s")

        let invalidEndHyphenCharReg = Regex("(-$)")
        let invalidEndEnDashCharReg = Regex("(‑$)")
        let invalidEndHyphenReg = Regex("(-$)")


        // ----------------------------------------------------------------------------

        // frequency10KPlus > 10,000
        let frequency10KPlus = 
            Set<string> 
                ["a"; "and"; "of"; "the"; "to";]

        // frequency8KPlus > 8,000
        let frequency8KPlus = 
            Set<string> 
                ["a"; "and"; "of"; "that"; "the"; "this"; "to";]

        // frequency6KPlus > 6,000
        let frequency6KPlus = 
            Set<string> 
                ["a"; "all"; "and"; "are"; "as"; "but"; "by"; "for"; "he"; "his"; "not"; "of"; "on"; "that"; "the"; "they"; "this";
                "to"; "up"; "was"; "with";] 

        // frequency4KPlus > 4,000
        let frequency4KPlus = 
            Set<string> 
                ["a"; "all"; "and"; "are"; "as"; "be"; "but"; "by"; "for"; "from"; "had"; "he"; "his"; "not"; "of"; "on"; "that"; 
                "the"; "their"; "these"; "they"; "this"; "to"; "up"; "was"; "with";] 

        // frequency2KPlus > 2,000
        let frequency2KPlus = 
            Set<string> 
                ["a"; "all"; "an"; "and"; "are"; "as"; "at"; "be"; "but"; "by"; "for"; "from"; "had"; "he"; "him"; "his"; "not"; 
                "of"; "on"; "one"; "or"; "so"; "that"; "the"; "their"; "these"; "they"; "this"; "to"; "up"; "was"; "with";] 

        // frequency1KPlus > 1,000
        let frequency1KPlus = 
            Set<string> 
                ["a"; "all"; "an"; "and"; "are"; "as"; "at"; "be"; "but"; "by"; "do"; "for"; "from"; "had"; "he"; "his"; "not"; "of"; 
                "on"; "or"; "no"; "so"; "that"; "the"; "these"; "this"; "to"; "up"; "was"; "with";] 
 
        let stopWords = 
            Set<string> 
                ["a"; "all"; "am"; "an"; "and"; "any"; "are"; "as"; "at"; "be"; "but"; "by"; "can"; "did"; "do"; 
                "for"; "he"; "his"; "i"; "ii"; "iii"; "iv"; "v"; "vi"; "vii"; "viii"; "ix"; "x"; "xi"; "xii"; "if"; 
                "in"; "is"; "it"; "its"; "me"; "my"; "no"; "nor"; "not"; "of"; "on"; "only"; "or"; 
                "our"; "put"; "so"; "the"; "to"; "up"; "us"; "use"; "was"; "we"; "why"; "yes";]  

        let aposwords = 
            Set<string>
                ["can’t"; "don’t"; "there’ll";]

        let noiseWords = 
            Set<string> 
                ["about"; "above"; "along"; "also"; "although"; "aren’t"; "because"; "been"; "cannot"; "can’t"; "could"; 
                "couldn’t"; "did"; "didn’t"; "don’t"; "does"; "doesn’t"; "e.g."; "either"; "etc."; "even"; "ever"; "from"; "further"; 
                "gets"; "hardly"; "had"; "has"; "hasn’t"; "having"; "hence"; "here"; "hereby"; "herein"; "hereof"; "hereon"; "hereto"; 
                "herewith"; "him"; "however"; "i.e."; "into"; "it’s"; "less"; "let"; "man’s"; "may"; "more"; "most"; "much"; "near"; 
                "one"; "onto"; "other"; "over"; "really"; "said"; "same";  "shall"; "should"; "shouldn’t"; "since"; "some"; "such"; 
                "than"; "that"; "their"; "them"; "then"; "there"; "there’ll"; "thereby"; "therefore"; "therefrom"; "therein"; "thereof"; 
                "thereon"; "thereto"; "therewith"; "these"; "they"; "this"; "those"; "through"; "thus"; "under"; "until"; "unto"; "upon"; 
                "used"; "very"; "wasn’t"; "were"; "what"; "when"; "where"; "whereby"; "wherein"; "whether"; "which"; "while"; "who"; "whom"; 
                "whose"; "with"; "without"; "would"; "won’t";  "you"; "your"; "have"; "thou"; "will";] 

        // ----------------------------------------------------------------------------

        let removeStopwords (stopwords: Set<string>) (word: string) =
            stopwords.Contains word = false

        let removeNoisewords (noisewords: Set<string>) (word: string) =
            noisewords.Contains word = false

        let removeEmptyStrings (word: string) =
            isEmptyStringReg.IsMatch(word) = false

        let isStopword (word: string) =
            stopWords.Contains word

        let isNoiseword (word: string) =
            noiseWords.Contains word

        let isChar (word: string) =
            word.Length = 1


        // ----------------------------------------------------------------------------

        let (|RegexMatch|_|) (pattern:string) (input:string) =
            let regex = Regex(pattern).Match(input)
            if regex.Success then Some(List.tail [ for x in regex.Groups -> x.Value])
            else None

        let (|RegexMatch3|_|) (pattern:string) (input:string) =
            let result = Regex.Match(input, pattern)
            if result.Success then
                match (List.tail [for g in result.Groups-> g.Value]) with
                | fst :: snd :: trd :: []
                     -> Some (fst, snd, trd)
                | [] -> failwith <| "Match succeeded, but no groups found.\n" +
                                     "Use '(.*)' to capture groups"
                | _ -> failwith "Match succeeded, but did not find exactly three groups."
            else
                None

        // ----------------------------------------------------------------------------


