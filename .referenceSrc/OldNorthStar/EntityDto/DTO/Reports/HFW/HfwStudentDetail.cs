using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.HFW
{
    public class OutputDto_HfwStudentDetailReportResult : OutputDto_Base
    {
        public OutputDto_HfwStudentDetailReportResult()
        {
            Sections = new List<WordListSection>();
        }
        public List<WordListSection> Sections { get; set; }
    }

    public class OutputDto_HfwStudentMissingWordsReportResult : OutputDto_Base
    {
        public OutputDto_HfwStudentMissingWordsReportResult()
        {
            WordsNotReadAndWritten = new List<HfwWordRow>();
        }
        public List<HfwWordRow> WordsNotReadAndWritten { get; set; }
    }

    public class WordListSubSection
    {
        public WordListSubSection()
        {
            Rows = new List<HfwWordRow>();
        }
        public List<HfwWordRow> Rows { get; set; }
    }

    public class MissingWordsSection
    {
        public MissingWordsSection()
        {
            Words = new List<HfwWordRow>();
            Start = 0;
            End = 0;
            IsKdg = false;
        }
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsKdg { get; set; }

        public string StudentName { get; set; }
        public string TeacherName { get; set; } // get this from last section to change total
        public string SchoolName { get; set; }

        public List<HfwWordRow> Words { get; set; }
    }

    public class WordListSection
    {
        public WordListSection()
        {
            LowerSection = new WordListSubSection();
            UpperSection = new WordListSubSection();
            TotalRead = 0;
            TotalScore = 0;
            TotalWritten = 0;
            Start = 0;
            End = 0;
            IsKdg = false;
        }
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsKdg { get; set; }
        public int WordCount { get; set; }

        public int TotalRead { get; set; }
        public int TotalWritten { get; set; }
        public int TotalScore { get; set; }

        public string StudentName { get; set; }
        public string TeacherName { get; set; } // get this from last section to change total
        public string SchoolName { get; set; }

        public WordListSubSection LowerSection { get; set; }
        public WordListSubSection UpperSection { get; set; }
    }

    public class HfwWordRow
    {
        public int WordId { get; set; }
        public int Order { get; set; }
        public string Word { get; set; }
        public DateTime? Read { get; set; }
        public DateTime? Write { get; set; }
        public bool WA { get; set; } // write assigned
        public bool RA { get; set; } // read assigned
    }
}
