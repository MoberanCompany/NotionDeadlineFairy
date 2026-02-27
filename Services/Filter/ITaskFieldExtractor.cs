public interface ITaskFieldExtractor<TTask>
{
    string GetFieldText(TTask task, string field);
    string GetAllFieldsText(TTask task);
}