namespace LC.Host.Domain.Courses;

public enum CourseStatus
{
    /// <summary>Черновик — виден только организации, потоки создавать нельзя.</summary>
    Draft = 1,

    /// <summary>Опубликован — виден в каталоге, можно открывать потоки.</summary>
    Published = 2,

    /// <summary>Архив — скрыт из каталога, новые потоки создавать нельзя.</summary>
    Archived = 3,
}
