using System;
using System.Collections.Generic;
using SokoSolve.Core.Game;

namespace SokoSolve.Core.Library.DB
{
    public class PuzzleDTO
    {
        public int PuzzleId { get; set; }
        public string Name { get; set; }
        public int Hash { get; set; }
        public int Rating { get; set; }
        public string CharMap { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string Author { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }

        public string SourceIdent { get; set; }

        public List<SolutionDTO> Solutions { get; set; }
    }

    public class SolutionDTO
    {
        public int SolutionId { get; set; }
        public int PuzzleREF { get; set; }

        public string CharPath { get; set; }

        public string HostMachine { get; set; }

        public int TotalNodes { get; set; }
        public decimal TotalSecs { get; set; }

        public string SolverType { get; set; }
        public int SolverVersionMajor { get; set; }
        public int SolverVersionMinor { get; set; }
        public string SolverDescription { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string Author { get; set; }
        public string URL { get; set; }
        public string Email { get; set; }

        public string Description { get; set; }
        public string Report { get; set; }
    }

    public interface ISokobanRepository
    {
        // Conversion
        PuzzleDTO ToDTO(Puzzle puzzle);

        // Reads
        PuzzleDTO Get(Puzzle puzzle);
        List<SolutionDTO> GetSolutions(int puzzleId);

        // Writes
        void Store(PuzzleDTO dto);
        void Store(SolutionDTO sol);
        void Update(SolutionDTO sol);
        PuzzleDTO Confirm(PuzzleDTO dto);
    }

//    public class SokoDBRepository
//    {
//        readonly DatabaseBinder puzzleBinder = new DatabaseBinder();
//        readonly DatabaseBinder solBinder = new DatabaseBinder();

//        PuzzleDTO Bind(IDataReader row)
//        {
//            var item = new PuzzleDTO();
//            item.PuzzleId = puzzleBinder.GetInt(row, "PuzzleId");
//            item.Name = puzzleBinder.GetString(row, "Name");
//            item.Hash = puzzleBinder.GetInt(row, "Hash");
//            item.Rating = puzzleBinder.GetInt(row, "Rating");
//            item.CharMap = puzzleBinder.GetString(row, "CharMap");
//            item.Created = puzzleBinder.GetDateTime(row, "Created");
//            item.Modified = puzzleBinder.GetDateTime(row, "Modified");
//            item.Author = puzzleBinder.GetString(row, "Author");
//            item.Url = puzzleBinder.GetString(row, "Url");
//            item.Email = puzzleBinder.GetString(row, "Email");
//            item.SourceIdent = puzzleBinder.GetString(row, "SourceIdent");
//            return item;
//        }

//        private SolutionDTO BindSolution(SqlDataReader row)
//        {
//            var binder = solBinder;
//            var item = new SolutionDTO();
//            item.SolutionId = binder.GetInt(row, "SolutionId");
//            item.PuzzleREF = binder.GetInt(row, "PuzzleREF");
//            item.CharPath = binder.GetString(row, "CharPath");
//            item.HostMachine = binder.GetString(row, "HostMachine");
//            item.TotalNodes = binder.GetInt(row, "TotalNodes");
//            item.TotalSecs = binder.GetDecimal(row, "TotalSecs");
//            item.SolverType = binder.GetString(row, "SolverType");
//            item.SolverVersionMajor = binder.GetInt(row, "SolverVersionMajor");
//            item.SolverVersionMinor = binder.GetInt(row, "SolverVersionMinor");
//            item.SolverDescription = binder.GetString(row, "SolverDescription");
//            item.Created = binder.GetDateTime(row, "Created");
//            item.Modified = binder.GetDateTime(row, "Modified");
//            item.Author = binder.GetString(row, "Author");
//            item.URL = binder.GetString(row, "URL");
//            item.Email = binder.GetString(row, "Email");
//            item.Description = binder.GetString(row, "Description");
//            item.Report = binder.GetString(row, "Report");
//            return item;
//        }

//        public List<PuzzleDTO> GetAll()
//        {
//            return DBHelper.ExecuteQuery(GetConnection(), Bind,
//                @"SELECT PuzzleId, Name, Hash, Rating, CharMap, Created, Modified, Author, Url, Email, SourceIdent FROM [Puzzle]"
//                );
//        }

//        public List<PuzzleDTO> GetAllAndSolutions()
//        {
//            var puz = DBHelper.ExecuteQuery(GetConnection(), Bind,
//                @"SELECT PuzzleId, Name, Hash, Rating, CharMap, Created, Modified, Author, Url, Email, SourceIdent FROM [Puzzle]"
//                );
//            var sol = GetSolutions();
//            foreach (var p in puz)
//            {
//                p.Solutions = sol.FindAll(x => x.PuzzleREF == p.PuzzleId);
//            }
//            return puz;
//        }

//        public PuzzleDTO Get(int id)
//        {
//            return DBHelper.ExecuteQuerySingle(GetConnection(), Bind,
//                @"SELECT PuzzleId, Name, Hash, Rating, CharMap, Created, Modified, Author, Url, Email, SourceIdent FROM [Puzzle] WHERE PuzzleId={0}",
//                id
//                );
//        }

//        public PuzzleDTO Get(string map)
//        {
//            return DBHelper.ExecuteQuerySingle(GetConnection(), Bind,
//                @"SELECT PuzzleId, Name, Hash, Rating, CharMap, Created, Modified, Author, Url, Email, SourceIdent FROM [Puzzle] WHERE CharMap={0}",
//                map
//                );
//        }
//        public PuzzleDTO Get(Puzzle puzzle) { return Get(puzzle.ToString()); }


//        public PuzzleDTO GetFull(int id)
//        {
//            var p = Get(id);
//            p.Solutions = GetSolutions(id);
//            return p;
//        }

//        public void Store(PuzzleDTO item)
//        {
//            item.PuzzleId = DBHelper.ExecuteScalarCommand<int>(GetConnection(),
//               @"INSERT INTO [Puzzle]
//( Name, Hash, Rating, CharMap, Created, Modified, Author, Url, Email, SourceIdent ) VALUES 
//(
//	{1},   -- Name
//	{2},   -- Hash
//	{3},   -- Rating
//	{4},   -- CharMap
//	{5},   -- Created
//	{6},   -- Modified
//	{7},   -- Author
//	{8},   -- Url
//	{9},   -- Email
//	{10}   -- SourceIdent
//);
//SELECT CAST(@@IDENTITY AS int);",
//item.PuzzleId, item.Name, item.Hash, item.Rating, item.CharMap, item.Created, item.Modified,
//item.Author, item.Url, item.Email, item.SourceIdent
//                );
//        }

//        public void Store(SolutionDTO item)
//        {
//            item.SolutionId = DBHelper.ExecuteScalarCommand<int>(GetConnection(),
//                @"INSERT INTO [Solution]
//(PuzzleREF, CharPath, HostMachine, TotalNodes, TotalSecs, SolverType, SolverVersionMajor, SolverVersionMinor, SolverDescription, 
//Created, Modified, Author, URL, Email, Description, Report ) VALUES 
//(	
//	{1},   -- PuzzleREF
//	{2},   -- CharPath
//	{3},   -- HostMachine
//	{4},   -- TotalNodes
//	{5},   -- TotalSecs
//	{6},   -- SolverType
//	{7},   -- SolverVersionMajor
//	{8},   -- SolverVersionMinor
//	{9},   -- SolverDescription
//	{10},   -- Created
//	{11},   -- Modified
//	{12},   -- Author
//	{13},   -- URL
//	{14},   -- Email
//	{15},   -- Description
//	{16}   -- Report
//);
//SELECT CAST(@@IDENTITY AS int);",
//                item.SolutionId, item.PuzzleREF, item.CharPath, item.HostMachine, item.TotalNodes,
//                item.TotalSecs, item.SolverType, item.SolverVersionMajor, item.SolverVersionMinor,
//                item.SolverDescription, item.Created, item.Modified, item.Author, item.URL, item.Email,
//                item.Description, item.Report);
//        }

//        public List<SolutionDTO> GetSolutions(int puzzleId)
//        {
//            return DBHelper.ExecuteQuery(GetConnection(), BindSolution,
//                @"SELECT SolutionId, PuzzleREF, CharPath, HostMachine, TotalNodes, TotalSecs, SolverType, 
//SolverVersionMajor, SolverVersionMinor, SolverDescription, Created, Modified, Author, 
//URL, Email, Description, Report
//FROM [Solution]
//WHERE PuzzleREF={0}", puzzleId);
//        }

//        public List<SolutionDTO> GetSolutions()
//        {
//            return DBHelper.ExecuteQuery(GetConnection(), BindSolution,
//                @"SELECT SolutionId, PuzzleREF, CharPath, HostMachine, TotalNodes, TotalSecs, SolverType, 
//SolverVersionMajor, SolverVersionMinor, SolverDescription, Created, Modified, Author, 
//URL, Email, Description, Report
//FROM [Solution]
//");
//        }


//        string GetConnection()
//        {
//            return "Data Source=OAK;Initial Catalog=SokoDB;User=netuser;password=rodman991";
//        }

//        public PuzzleDTO ToDTO(Puzzle puzzle)
//        {
//            return new PuzzleDTO()
//            {
//                CharMap = puzzle.ToString(),
//                Name = PuzzleHelper.GetName(puzzle),
//                Rating = (int)StaticAnalysis.CalculateRating(puzzle),
//            };
//        }


//        public Puzzle ToDomain(PuzzleDTO dto)
//        {
//            return new Puzzle(dto.CharMap)
//            {
//                Name = dto.Name,
//                Tag = dto
//            };
//        }


//        public void Update(SolutionDTO item)
//        {
//           DBHelper.ExecuteCommand(GetConnection(),
//              @"UPDATE [Solution] SET 	
//	[PuzzleREF]={1},		-- int NOT NULL
//	[CharPath]={2},		-- nvarchar(500)
//	[HostMachine]={3},		-- nvarchar(500)
//	[TotalNodes]={4},		-- int NOT NULL
//	[TotalSecs]={5},		-- numeric NOT NULL
//	[SolverType]={6},		-- nvarchar(500)
//	[SolverVersionMajor]={7},		-- int NOT NULL
//	[SolverVersionMinor]={8},		-- int NOT NULL
//	[SolverDescription]={9},		-- nvarchar(500)
//	[Created]={10},		-- datestamp NOT NULL
//	[Modified]={11},		-- datestamp NOT NULL
//	[Author]={12},		-- nvarchar(500)
//	[URL]={13},		-- nvarchar(500)
//	[Email]={14},		-- nvarchar(500)
//	[Description]={15},		-- nvarchar(500)
//	[Report]={16}		-- nvarchar(500)
//WHERE SolutionId={0}",
//item.SolutionId, item.PuzzleREF, item.CharPath, item.HostMachine, item.TotalNodes,
//item.TotalSecs, item.SolverType, item.SolverVersionMajor, item.SolverVersionMinor,
//item.SolverDescription, item.Created, item.Modified, item.Author, item.URL, item.Email,
//item.Description, item.Report
//);
//        }

//        public PuzzleDTO Confirm(PuzzleDTO dto)
//        {
//            var exist = Get(dto.CharMap);
//            if (exist != null) return exist;

//            Store(dto);
//            return dto;

//        }

//    }
}