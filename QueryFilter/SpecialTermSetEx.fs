﻿namespace QueryFilterLib

open RecordTypes

    module SpecialTermSetEx =

        let SpecialTermRecordSetEx =
            Set<TermLocationRecordEx>
                [|
                {Term="A.D.";Pattern="{term}";LocationsEx=[|{did=31;sid=93;idx=528;len=4;pid="31:10.22";};{did=41;sid=26;idx=141;len=4;pid="41:3.5";};{did=62;sid=37;idx=14;len=4;pid="62:5.1";};{did=62;sid=62;idx=44;len=4;pid="62:7.7";};{did=63;sid=51;idx=40;len=4;pid="63:6.8";};{did=74;sid=2;idx=47;len=4;pid="74:0.1";};{did=79;sid=12;idx=100;len=4;pid="79:1.9";};{did=85;sid=8;idx=1013;len=4;pid="85:1.2";};{did=119;sid=71;idx=501;len=4;pid="119:8.9";};{did=121;sid=24;idx=223;len=4;pid="121:2.11";};{did=121;sid=89;idx=855;len=4;pid="121:8.3";};{did=121;sid=89;idx=1033;len=4;pid="121:8.3";};{did=121;sid=92;idx=53;len=4;pid="121:8.6";};{did=121;sid=92;idx=229;len=4;pid="121:8.6";};{did=121;sid=93;idx=33;len=4;pid="121:8.7";};{did=121;sid=94;idx=229;len=4;pid="121:8.8";};{did=121;sid=94;idx=633;len=4;pid="121:8.8";};{did=121;sid=95;idx=486;len=4;pid="121:8.9";};{did=121;sid=96;idx=508;len=4;pid="121:8.10";};{did=123;sid=44;idx=21;len=4;pid="123:4.0";};{did=123;sid=53;idx=88;len=4;pid="123:4.9";};{did=123;sid=70;idx=20;len=4;pid="123:6.0";};{did=123;sid=77;idx=78;len=4;pid="123:6.7";};{did=124;sid=3;idx=22;len=4;pid="124:1.0";};{did=124;sid=15;idx=272;len=4;pid="124:1.12";};{did=124;sid=17;idx=19;len=4;pid="124:2.0";};{did=124;sid=28;idx=22;len=4;pid="124:3.0";};{did=124;sid=32;idx=31;len=4;pid="124:3.4";};{did=124;sid=39;idx=21;len=4;pid="124:4.0";};{did=124;sid=49;idx=24;len=4;pid="124:5.0";};{did=124;sid=51;idx=28;len=4;pid="124:5.2";};{did=124;sid=53;idx=40;len=4;pid="124:5.4";};{did=124;sid=57;idx=301;len=4;pid="124:6.1";};{did=126;sid=6;idx=24;len=4;pid="126:1.0";};{did=126;sid=23;idx=23;len=4;pid="126:3.0";};{did=126;sid=25;idx=32;len=4;pid="126:3.2";};{did=127;sid=6;idx=23;len=4;pid="127:1.0";};{did=127;sid=15;idx=25;len=4;pid="127:2.0";};{did=127;sid=28;idx=24;len=4;pid="127:3.0";};{did=127;sid=44;idx=24;len=4;pid="127:4.0";};{did=127;sid=62;idx=23;len=4;pid="127:6.0";};{did=128;sid=7;idx=26;len=4;pid="128:1.0";};{did=128;sid=23;idx=27;len=4;pid="128:2.0";};{did=128;sid=31;idx=26;len=4;pid="128:3.0";};{did=128;sid=51;idx=27;len=4;pid="128:5.0";};{did=128;sid=61;idx=26;len=4;pid="128:6.0";};{did=128;sid=74;idx=26;len=4;pid="128:7.0";};{did=129;sid=5;idx=28;len=4;pid="129:1.0";};{did=129;sid=6;idx=25;len=4;pid="129:1.1";};{did=129;sid=21;idx=27;len=4;pid="129:2.0";};{did=129;sid=22;idx=10;len=4;pid="129:2.1";};{did=129;sid=33;idx=26;len=4;pid="129:3.0";};{did=130;sid=2;idx=228;len=4;pid="130:0.1";};{did=130;sid=2;idx=421;len=4;pid="130:0.1";};{did=134;sid=4;idx=23;len=4;pid="134:1.0";};{did=134;sid=5;idx=64;len=4;pid="134:1.1";};{did=134;sid=8;idx=427;len=4;pid="134:1.4";};{did=134;sid=13;idx=27;len=4;pid="134:2.1";};{did=134;sid=17;idx=380;len=4;pid="134:2.5";};{did=134;sid=74;idx=26;len=4;pid="134:7.0";};{did=134;sid=83;idx=295;len=4;pid="134:8.1";};{did=134;sid=101;idx=460;len=4;pid="134:9.8";};{did=135;sid=13;idx=59;len=4;pid="135:2.1";};{did=135;sid=23;idx=14;len=4;pid="135:4.1";};{did=135;sid=28;idx=213;len=4;pid="135:4.6";};{did=135;sid=39;idx=29;len=4;pid="135:6.1";};{did=135;sid=52;idx=15;len=4;pid="135:8.1";};{did=135;sid=57;idx=182;len=4;pid="135:8.6";};{did=135;sid=71;idx=196;len=4;pid="135:10.2";};{did=136;sid=19;idx=184;len=4;pid="136:2.8";};{did=136;sid=19;idx=215;len=4;pid="136:2.8";};{did=136;sid=19;idx=393;len=4;pid="136:2.8";};{did=136;sid=19;idx=473;len=4;pid="136:2.8";};{did=137;sid=2;idx=40;len=4;pid="137:0.1";};{did=137;sid=13;idx=29;len=4;pid="137:2.1";};{did=137;sid=54;idx=105;len=4;pid="137:6.1";};{did=137;sid=78;idx=73;len=4;pid="137:8.3";};{did=138;sid=5;idx=31;len=4;pid="138:1.1";};{did=138;sid=53;idx=176;len=4;pid="138:7.7";};{did=138;sid=81;idx=250;len=4;pid="138:10.11";};{did=139;sid=8;idx=3;len=4;pid="139:1.2";};{did=139;sid=60;idx=381;len=4;pid="139:4.15";};{did=140;sid=2;idx=40;len=4;pid="140:0.1";};{did=141;sid=2;idx=42;len=4;pid="141:0.1";};{did=143;sid=2;idx=20;len=4;pid="143:0.1";};{did=144;sid=181;idx=293;len=4;pid="144:6.13";};{did=144;sid=197;idx=86;len=4;pid="144:9.1";};{did=144;sid=198;idx=49;len=4;pid="144:9.2";};{did=145;sid=55;idx=251;len=4;pid="145:5.8";};{did=146;sid=2;idx=72;len=4;pid="146:0.1";};{did=148;sid=2;idx=25;len=4;pid="148:0.1";};{did=149;sid=2;idx=72;len=4;pid="149:0.1";};{did=150;sid=2;idx=31;len=4;pid="150:0.1";};{did=154;sid=10;idx=15;len=4;pid="154:2.1";};{did=154;sid=44;idx=76;len=4;pid="154:7.1";};{did=156;sid=62;idx=54;len=4;pid="156:6.9";};{did=158;sid=2;idx=52;len=4;pid="158:0.1";};{did=161;sid=2;idx=25;len=4;pid="161:0.1";};{did=165;sid=2;idx=23;len=4;pid="165:0.1";};{did=166;sid=54;idx=83;len=4;pid="166:5.7";};{did=172;sid=2;idx=100;len=4;pid="172:0.1";};{did=175;sid=35;idx=452;len=4;pid="175:3.1";};{did=185;sid=2;idx=59;len=4;pid="185:0.1";};{did=189;sid=6;idx=404;len=4;pid="189:1.1";};|]};
                {Term="B.C.";Pattern="{term}";LocationsEx=[|{did=74;sid=92;idx=278;len=4;pid="74:8.11";};{did=77;sid=48;idx=13;len=4;pid="77:4.13";};{did=78;sid=28;idx=28;len=4;pid="78:3.4";};{did=78;sid=33;idx=247;len=4;pid="78:3.9";};{did=78;sid=33;idx=317;len=4;pid="78:3.9";};{did=78;sid=46;idx=10;len=4;pid="78:5.4";};{did=78;sid=50;idx=103;len=4;pid="78:5.8";};{did=78;sid=52;idx=80;len=4;pid="78:6.1";};{did=78;sid=69;idx=225;len=4;pid="78:8.1";};{did=78;sid=78;idx=11;len=4;pid="78:8.10";};{did=78;sid=79;idx=18;len=4;pid="78:8.11";};{did=79;sid=6;idx=8;len=4;pid="79:1.3";};{did=79;sid=16;idx=10;len=4;pid="79:2.3";};{did=79;sid=17;idx=13;len=4;pid="79:2.4";};{did=79;sid=26;idx=19;len=4;pid="79:3.4";};{did=79;sid=29;idx=151;len=4;pid="79:3.7";};{did=79;sid=49;idx=263;len=4;pid="79:5.8";};{did=79;sid=59;idx=233;len=4;pid="79:6.8";};{did=79;sid=60;idx=22;len=4;pid="79:6.9";};{did=79;sid=77;idx=21;len=4;pid="79:8.5";};{did=80;sid=44;idx=8;len=4;pid="80:5.8";};{did=80;sid=53;idx=13;len=4;pid="80:7.2";};{did=80;sid=59;idx=14;len=4;pid="80:7.8";};{did=80;sid=60;idx=8;len=4;pid="80:7.9";};{did=80;sid=61;idx=11;len=4;pid="80:7.10";};{did=80;sid=77;idx=136;len=4;pid="80:9.6";};{did=80;sid=78;idx=8;len=4;pid="80:9.7";};{did=80;sid=80;idx=134;len=4;pid="80:9.9";};{did=80;sid=83;idx=76;len=4;pid="80:9.12";};{did=80;sid=86;idx=400;len=4;pid="80:9.15";};{did=81;sid=25;idx=168;len=4;pid="81:2.12";};{did=81;sid=40;idx=315;len=4;pid="81:3.6";};{did=92;sid=37;idx=479;len=4;pid="92:4.6";};{did=93;sid=5;idx=239;len=4;pid="93:1.1";};{did=95;sid=4;idx=8;len=4;pid="95:1.1";};{did=122;sid=16;idx=48;len=4;pid="122:2.7";};{did=122;sid=45;idx=25;len=4;pid="122:7.1";};{did=122;sid=45;idx=590;len=4;pid="122:7.1";};{did=122;sid=45;idx=663;len=4;pid="122:7.1";};{did=122;sid=60;idx=161;len=4;pid="122:8.7";};{did=122;sid=60;idx=178;len=4;pid="122:8.7";};{did=123;sid=5;idx=162;len=4;pid="123:0.4";};{did=123;sid=13;idx=144;len=4;pid="123:1.5";};{did=123;sid=17;idx=473;len=4;pid="123:2.1";};{did=124;sid=15;idx=258;len=4;pid="124:1.12";};|]};
                {Term="2 x 1027";Pattern="{term}";LocationsEx=[|{did=41;sid=33;idx=124;len=8;pid="41:4.1";};|]};
                {Term="Shalmaneser III";Pattern="{term}";LocationsEx=[|{did=97;sid=87;idx=258;len=15;pid="97:9.18";};|]};
                {Term="Amenhotep III";Pattern="{term}";LocationsEx=[|{did=111;sid=6;idx=370;len=13;pid="111:0.5";};|]};
                {Term="1-2-3";Pattern="{term}";LocationsEx=[|{did=77;sid=78;idx=110;len=5;pid="77:8.2";};
                                                             {did=77;sid=94;idx=333;len=5;pid="77:9.4";};
                                                             {did=77;sid=95;idx=0;len=5;pid="77:9.5";};|]};
                {Term="4-5-6";Pattern="{term}";LocationsEx=[|{did=77;sid=78;idx=127;len=5;pid="77:8.2";};|]};
                {Term="A-B-C";Pattern="{term}";LocationsEx=[|{did=77;sid=64;idx=215;len=5;pid="77:6.4";};|]};
                {Term="D-E-F";Pattern="{term}";LocationsEx=[|{did=77;sid=64;idx=277;len=5;pid="77:6.4";};|]};
                |]

