namespace QueryFilter

open RecordTypes

    module HypenEndingTermSet =

        // Keyboard Hypyen
        let keyboardHypen = "-"
        // u+2011 (or char(8209)) non-breaking hyphen copy paste: ‑
        let nonBreakingHyphen = "‑"

        let HypenEndingTermSet = 
            Set<string * TermLocationListEx>
                [|
                ("mortal‑", [{did=0; sid=95; idx=267; len=7; pid="0:5.1"};]);
                ("self‑", [{did=5; sid=27; idx=47; len=5; pid="5:3.3"};]);
                ("inter‑", [{did=7; sid=11; idx=211; len=6; pid="7:1.4"};]);
                ("super‑", [{did=11; sid=61; idx=473; len=6; pid="11:8.3"};
                            {did=17; sid=68; idx=744; len=6; pid="17:8.1"};
                            {did=22; sid=67; idx=612; len=6; pid="22:7.10"};
                            {did=23; sid=42; idx=299; len=6; pid="23:3.2"};
                            {did=26; sid=18; idx=371; len=6; pid="26:1.15"};
                            {did=30; sid=289; idx=703; len=6; pid="30:3.12"};
                            {did=33; sid=30; idx=352; len=6; pid="33:4.6"};
                            {did=36; sid=46; idx=108; len=6; pid="36:4.7"};
                            {did=37; sid=39; idx=171; len=6; pid="37:4.3"};
                            {did=37; sid=77; idx=12; len=6; pid="37:9.1"};
                            {did=41; sid=3; idx=131; len=6; pid="41:0.2"};]);
                ("physical‑", [{did=12; sid=74; idx=260; len=9; pid="12:6.3"};]);
                ("right‑", [{did=15; sid=69; idx=150; len=6; pid="15:5.4"};]);

                ("Son‑", [{did=18; sid=40; idx=421; len=4; pid="18:4.5"};
                          {did=40; sid=83; idx=294; len=4; pid="40:10.8"};
                          {did=40; sid=85; idx=395; len=4; pid="40:10.10"};]);
                ("Trinity‑", [{did=19; sid=12; idx=294; len=8; pid="19:1.1"};]);
                ("Spirit‑", [{did=22; sid=54; idx=292; len=7; pid="22:6.1"};]);
                ("single‑", [{did=23; sid=54; idx=570; len=7; pid="23:4.4"};]);
                ("general‑", [{did=33; sid=32; idx=70; len=8; pid="33:4.8"};]);
                ("worship‑", [{did=34; sid=43; idx=361; len=8; pid="34:5.3"};]);
                ("morontia‑", [{did=38; sid=55; idx=278; len=9; pid="38:9.8"};]);
                ("one‑", [{did=40; sid=36; idx=201; len=4; pid="40:5.12"};
                          {did=49; sid=52; idx=169; len=4; pid="49:4.3"};
                          {did=49; sid=72; idx=173; len=4; pid="49:5.13"};
                          {did=49; sid=76; idx=101; len=4; pid="49:5.17"};
                          {did=49; sid=78; idx=152; len=4; pid="49:5.19"};]);
                ("two‑", [{did=40; sid=36; idx=207; len=4; pid="40:5.12"};
                         {did=49; sid=72; idx=183; len=4; pid="49:5.13"};
                         {did=49; sid=78; idx=158; len=4; pid="49:5.19"};]);
                ("sub‑", [{did=49; sid=26; idx=391; len=4; pid="49:2.11"};]);
                ("fresh‑", [{did=60; sid=11; idx=289; len=6; pid="60:1.7"}]);
                ("salt‑", [{did=60; sid=24; idx=209; len=5; pid="60:2.5"}]);
                ("mother‑", [{did=79; sid=87; idx=291; len=7; pid="79:8.15"}]);
                |]