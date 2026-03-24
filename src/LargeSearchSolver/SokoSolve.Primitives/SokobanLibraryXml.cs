using System.Xml.Serialization;

namespace SokoSolve.Primitives.Xml;

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
[XmlRoot(Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd", IsNullable=false)]
public partial class SokobanLibrary
{
    public GenericDescription? Details;

    [XmlArrayItem("Category", IsNullable=false)]
    public SokobanLibraryCategory[]? Categories;

    [XmlArrayItem("Puzzle", IsNullable=false)]
    public SokobanLibraryPuzzle[]? Puzzles;

    [XmlAttribute()]
    public string? LibraryID;

    [XmlAttribute()]
    public string? Rating;

    [XmlAttribute()]
    public int MaxID;
}

[Serializable]
[XmlType(Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class GenericDescription
{
    public string? Name;
    public string? Description;
    public string? Comments;
    public GenericDescriptionAuthor? Author;

    [XmlAttribute()]
    public string? License;

    [XmlAttribute()]
    public DateTime Date;

    [XmlIgnore()]
    public bool DateSpecified;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class GenericDescriptionAuthor
{
    [XmlAttribute()]
    public string? Name;

    [XmlAttribute()]
    public string? Email;

    [XmlAttribute()]
    public string? Homepage;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class SokobanLibraryCategory
{
    public GenericDescription? CategoryDescription;

    [XmlAttribute(DataType="ID")]
    public string? CategoryID;

    [XmlAttribute(DataType="IDREF")]
    public string? CategoryParentREF;

    [XmlAttribute()]
    public int Order;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class SokobanLibraryPuzzle
{
    public GenericDescription? PuzzleDescription;

    [XmlArrayItem("Map", IsNullable=false)]
    public SokobanLibraryPuzzleMap[]? Maps;

    [XmlAttribute()]
    public string? Rating;

    [XmlAttribute(DataType="ID")]
    public string? PuzzleID;

    [XmlAttribute(DataType="IDREF")]
    public string? CategoryREF;

    [XmlAttribute()]
    public int Order;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class SokobanLibraryPuzzleMap
{
    [XmlElement("Row")]
    public string[]? Row;

    [XmlArrayItem("Solution", IsNullable=false)]
    public SokobanLibraryPuzzleMapSolution[]? Solutions;

    public GenericDescription? MapDetails;

    [XmlArrayItem("Hint", IsNullable=false)]
    public SokobanLibraryPuzzleMapHint[]? Hints;

    [XmlAttribute(DataType="ID")]
    public string? MapID;

    [XmlAttribute()]
    public string? Rating;

    [XmlAttribute()]
    public string? MapType;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class SokobanLibraryPuzzleMapSolution
{
    public string? Steps;
    public GenericDescription? SolutionDescription;

    [XmlAttribute()]
    public int StartX;

    [XmlAttribute()]
    public int StartY;
}

[Serializable]
[XmlType(AnonymousType=true, Namespace="http://sokosolve.sourceforge.net/SokoSolveLibrary.xsd")]
public partial class SokobanLibraryPuzzleMapHint
{
    [XmlAttribute(DataType="language")]
    public string? X;

    [XmlAttribute(DataType="integer")]
    public string? Y;

    [XmlAttribute()]
    public string? Type;

    [XmlAttribute()]
    public string? Text;
}
