using BugCore.Core.Entities;

namespace BugCore.Core.Interfaces;

public interface ICustomFieldService
{
    Task<IEnumerable<CustomField>> GetAllFieldsAsync();
    Task<IEnumerable<CustomField>> GetFieldsForProjectAsync(int projectId);
    Task<IEnumerable<CustomFieldProject>> GetLinkedFieldsForProjectAsync(int projectId);
    Task<IEnumerable<CustomField>> GetUnlinkedFieldsForProjectAsync(int projectId);
    Task<CustomField?> GetFieldByIdAsync(int id);
    Task<CustomField> CreateFieldAsync(CustomField field);
    Task<CustomField> UpdateFieldAsync(CustomField field);
    Task<bool> DeleteFieldAsync(int id);
    Task<bool> LinkFieldToProjectAsync(int fieldId, int projectId, int sequence);
    Task<bool> UpdateFieldSequenceAsync(int fieldId, int projectId, int sequence);
    Task<bool> UnlinkFieldFromProjectAsync(int fieldId, int projectId);
    Task<bool> CopyFieldsAsync(int sourceProjectId, int targetProjectId);
}
