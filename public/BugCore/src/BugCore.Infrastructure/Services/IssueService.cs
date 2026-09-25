using Microsoft.EntityFrameworkCore;
using BugCore.Core.Entities;
using BugCore.Core.Enums;
using BugCore.Core.Interfaces;
using BugCore.Infrastructure.Data;

namespace BugCore.Infrastructure.Services;

public class IssueService : IIssueService
{
    private readonly BugCoreDbContext _context;

    public IssueService(BugCoreDbContext context)
    {
        _context = context;
    }

    public async Task<Issue?> GetIssueByIdAsync(int id, bool includeDetails = true)
    {
        var query = _context.Issues
            .Include(i => i.Project)
            .Include(i => i.Reporter)
            .Include(i => i.Handler)
            .Include(i => i.Category)
            .AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(i => i.Notes.OrderBy(n => n.DateSubmitted)).ThenInclude(n => n.Reporter)
                .Include(i => i.History.OrderByDescending(h => h.DateModified)).ThenInclude(h => h.User)
                .Include(i => i.Monitors).ThenInclude(m => m.User)
                .Include(i => i.Attachments.OrderByDescending(a => a.DateAdded)).ThenInclude(a => a.User)
                .Include(i => i.SourceRelationships).ThenInclude(r => r.DestinationIssue)
                .Include(i => i.DestinationRelationships).ThenInclude(r => r.SourceIssue)
                .Include(i => i.CustomValues).ThenInclude(cv => cv.CustomField);
        }

        return await query.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Issue>> GetIssuesAsync(IssueFilterCriteria criteria)
    {
        var query = BuildIssueFilterQuery(criteria);

        // Sorting
        query = criteria.SortField?.ToLower() switch
        {
            "id" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Id) : query.OrderByDescending(i => i.Id),
            "priority" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Priority) : query.OrderByDescending(i => i.Priority),
            "severity" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Severity) : query.OrderByDescending(i => i.Severity),
            "status" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Status) : query.OrderByDescending(i => i.Status),
            "summary" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Summary) : query.OrderByDescending(i => i.Summary),
            "category" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.Category!.Name) : query.OrderByDescending(i => i.Category!.Name),
            "datesubmitted" => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.DateSubmitted) : query.OrderByDescending(i => i.DateSubmitted),
            _ => criteria.SortDirection?.ToUpper() == "ASC" ? query.OrderBy(i => i.LastUpdated) : query.OrderByDescending(i => i.LastUpdated)
        };

        int skip = Math.Max(0, (criteria.Page - 1) * criteria.PageSize);
        int take = Math.Max(1, criteria.PageSize);

        return await query
            .Include(i => i.Project)
            .Include(i => i.Reporter)
            .Include(i => i.Handler)
            .Include(i => i.Category)
            .Include(i => i.Notes)
            .Include(i => i.Attachments)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetIssueCountAsync(IssueFilterCriteria criteria)
    {
        var query = BuildIssueFilterQuery(criteria);
        return await query.CountAsync();
    }

    private IQueryable<Issue> BuildIssueFilterQuery(IssueFilterCriteria criteria)
    {
        var query = _context.Issues.AsQueryable();

        if (criteria.ProjectId.HasValue && criteria.ProjectId.Value > 0)
        {
            query = query.Where(i => i.ProjectId == criteria.ProjectId.Value);
        }

        if (criteria.CategoryId.HasValue && criteria.CategoryId.Value > 0)
        {
            query = query.Where(i => i.CategoryId == criteria.CategoryId.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(i => i.Status == criteria.Status.Value);
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(i => i.Priority == criteria.Priority.Value);
        }

        if (criteria.Severity.HasValue)
        {
            query = query.Where(i => i.Severity == criteria.Severity.Value);
        }

        if (criteria.Resolution.HasValue)
        {
            query = query.Where(i => i.Resolution == criteria.Resolution.Value);
        }

        if (criteria.HandlerId.HasValue)
        {
            if (criteria.HandlerId.Value == 0 || criteria.HandlerId.Value == -1)
            {
                query = query.Where(i => i.HandlerId == null);
            }
            else
            {
                query = query.Where(i => i.HandlerId == criteria.HandlerId.Value);
            }
        }

        if (criteria.ReporterId.HasValue && criteria.ReporterId.Value > 0)
        {
            query = query.Where(i => i.ReporterId == criteria.ReporterId.Value);
        }

        if (criteria.ViewState.HasValue)
        {
            query = query.Where(i => i.ViewState == criteria.ViewState.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var s = criteria.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Summary.ToLower().Contains(s) ||
                i.Description.ToLower().Contains(s) ||
                (i.Category != null && i.Category.Name.ToLower().Contains(s)) ||
                (i.Reporter != null && i.Reporter.UserName != null && i.Reporter.UserName.ToLower().Contains(s)) ||
                (i.Handler != null && i.Handler.UserName != null && i.Handler.UserName.ToLower().Contains(s)));
        }

        return query;
    }

    public async Task<Issue> CreateIssueAsync(Issue issue, int reporterUserId, IDictionary<int, string>? customFields = null)
    {
        issue.ReporterId = reporterUserId;
        issue.DateSubmitted = DateTime.UtcNow;
        issue.LastUpdated = DateTime.UtcNow;

        if (string.IsNullOrEmpty(issue.SlackMessageTs))
        {
            var proj = await _context.Projects.FindAsync(issue.ProjectId);
            if (proj != null)
            {
                issue.SlackChannelId = proj.SlackChannelId ?? "C08TRIAGE00";
                issue.SlackMessageTs = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.{Random.Shared.Next(100000, 999999)}";
                issue.SlackThreadTs = issue.SlackMessageTs;
            }
        }

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issue.Id,
            UserId = reporterUserId,
            FieldName = "Issue Created",
            NewValue = $"Status: {issue.Status}, Severity: {issue.Severity}, Priority: {issue.Priority}",
            Type = 1,
            DateModified = DateTime.UtcNow
        });

        if (customFields != null && customFields.Any())
        {
            foreach (var kvp in customFields)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    _context.CustomFieldValues.Add(new CustomFieldValue
                    {
                        IssueId = issue.Id,
                        CustomFieldId = kvp.Key,
                        Value = kvp.Value
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<Issue> UpdateIssueAsync(Issue issue, int currentUserId)
    {
        issue.LastUpdated = DateTime.UtcNow;
        _context.Issues.Update(issue);

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issue.Id,
            UserId = currentUserId,
            FieldName = "Issue Updated",
            NewValue = "General field modification",
            DateModified = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<bool> DeleteIssueAsync(int id, int currentUserId)
    {
        var issue = await _context.Issues.FindAsync(id);
        if (issue == null) return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int issueId, IssueStatus newStatus, ResolutionType? resolution, string? noteText, int currentUserId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        var oldStatus = issue.Status;
        issue.Status = newStatus;
        if (resolution.HasValue)
        {
            issue.Resolution = resolution.Value;
        }
        else if (newStatus == IssueStatus.Resolved && issue.Resolution == ResolutionType.Open)
        {
            issue.Resolution = ResolutionType.Fixed;
        }

        issue.LastUpdated = DateTime.UtcNow;

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = "Status",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString(),
            Type = 0,
            DateModified = DateTime.UtcNow
        });

        if (!string.IsNullOrWhiteSpace(noteText))
        {
            _context.IssueNotes.Add(new IssueNote
            {
                IssueId = issueId,
                ReporterId = currentUserId,
                Note = noteText,
                DateSubmitted = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignHandlerAsync(int issueId, int? handlerId, int currentUserId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        var oldHandlerId = issue.HandlerId;
        issue.HandlerId = (handlerId.HasValue && handlerId.Value > 0) ? handlerId.Value : null;
        issue.LastUpdated = DateTime.UtcNow;

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = "Assigned To",
            OldValue = oldHandlerId?.ToString() ?? "Unassigned",
            NewValue = issue.HandlerId?.ToString() ?? "Unassigned",
            DateModified = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IssueNote> AddNoteAsync(int issueId, string noteText, bool isPrivate, int timeTrackingMinutes, int currentUserId)
    {
        var note = new IssueNote
        {
            IssueId = issueId,
            ReporterId = currentUserId,
            Note = noteText,
            ViewState = isPrivate ? ProjectViewState.Private : ProjectViewState.Public,
            TimeTrackingMinutes = timeTrackingMinutes,
            DateSubmitted = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _context.IssueNotes.Add(note);

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = isPrivate ? "Private Bugnote Added" : "Bugnote Added",
            NewValue = noteText.Length > 50 ? noteText.Substring(0, 47) + "..." : noteText,
            Type = 2,
            DateModified = DateTime.UtcNow
        });

        var issue = await _context.Issues.FindAsync(issueId);
        if (issue != null)
        {
            issue.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteNoteAsync(int noteId, int currentUserId)
    {
        var note = await _context.IssueNotes.FindAsync(noteId);
        if (note == null) return false;

        _context.IssueNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleMonitorAsync(int issueId, int userId)
    {
        var monitor = await _context.IssueMonitors
            .FirstOrDefaultAsync(m => m.IssueId == issueId && m.UserId == userId);

        if (monitor != null)
        {
            _context.IssueMonitors.Remove(monitor);
            await _context.SaveChangesAsync();
            return false;
        }
        else
        {
            _context.IssueMonitors.Add(new IssueMonitor
            {
                IssueId = issueId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public async Task<bool> IsMonitoredByUserAsync(int issueId, int userId)
    {
        return await _context.IssueMonitors.AnyAsync(m => m.IssueId == issueId && m.UserId == userId);
    }

    public async Task<bool> ToggleStickyAsync(int issueId, bool isSticky, int currentUserId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        issue.Sticky = isSticky;
        issue.LastUpdated = DateTime.UtcNow;

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = "Sticky",
            OldValue = (!isSticky).ToString(),
            NewValue = isSticky.ToString(),
            DateModified = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddMonitorUserAsync(int issueId, int userId)
    {
        var exists = await _context.IssueMonitors.AnyAsync(m => m.IssueId == issueId && m.UserId == userId);
        if (exists) return true;

        _context.IssueMonitors.Add(new IssueMonitor
        {
            IssueId = issueId,
            UserId = userId
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMonitorUserAsync(int issueId, int userId)
    {
        var monitor = await _context.IssueMonitors
            .FirstOrDefaultAsync(m => m.IssueId == issueId && m.UserId == userId);

        if (monitor == null) return false;

        _context.IssueMonitors.Remove(monitor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddRelationshipAsync(int sourceIssueId, int targetIssueId, RelationshipType type, int currentUserId)
    {
        if (sourceIssueId == targetIssueId) return false;

        var exists = await _context.IssueRelationships.AnyAsync(r =>
            (r.SourceIssueId == sourceIssueId && r.DestinationIssueId == targetIssueId) ||
            (r.SourceIssueId == targetIssueId && r.DestinationIssueId == sourceIssueId));

        if (exists) return false;

        var relationship = new IssueRelationship
        {
            SourceIssueId = sourceIssueId,
            DestinationIssueId = targetIssueId,
            Type = type
        };

        _context.IssueRelationships.Add(relationship);

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = sourceIssueId,
            UserId = currentUserId,
            FieldName = "Relationship Added",
            NewValue = $"{type} Issue #{targetIssueId:D7}",
            Type = 3,
            DateModified = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRelationshipAsync(int relationshipId, int currentUserId)
    {
        var rel = await _context.IssueRelationships.FindAsync(relationshipId);
        if (rel == null) return false;

        _context.IssueRelationships.Remove(rel);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IssueAttachment> AddAttachmentAsync(int issueId, string fileName, string fileType, byte[] content, string description, int currentUserId)
    {
        var attachment = new IssueAttachment
        {
            IssueId = issueId,
            UserId = currentUserId,
            FileName = fileName,
            FileType = fileType,
            FileSize = content.Length,
            Content = content,
            Description = description,
            DateAdded = DateTime.UtcNow
        };

        _context.IssueAttachments.Add(attachment);

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = "File Added",
            NewValue = $"{fileName} ({content.Length / 1024} KB)",
            DateModified = DateTime.UtcNow
        });

        var issue = await _context.Issues.FindAsync(issueId);
        if (issue != null)
        {
            issue.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<IssueAttachment?> GetAttachmentByIdAsync(int attachmentId)
    {
        return await _context.IssueAttachments
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId, int currentUserId)
    {
        var attachment = await _context.IssueAttachments.FindAsync(attachmentId);
        if (attachment == null) return false;

        var issueId = attachment.IssueId;
        var fileName = attachment.FileName;

        _context.IssueAttachments.Remove(attachment);

        _context.IssueHistories.Add(new IssueHistory
        {
            IssueId = issueId,
            UserId = currentUserId,
            FieldName = "File Deleted",
            OldValue = fileName,
            DateModified = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    // Batch Action Groups (bug_actiongroup.php)
    public async Task<bool> BatchChangeStatusAsync(IEnumerable<int> issueIds, IssueStatus status, ResolutionType? resolution, string? noteText, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var issue in issues)
        {
            var oldStatus = issue.Status;
            issue.Status = status;
            if (resolution.HasValue)
            {
                issue.Resolution = resolution.Value;
            }
            else if (status == IssueStatus.Resolved && issue.Resolution == ResolutionType.Open)
            {
                issue.Resolution = ResolutionType.Fixed;
            }
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "Status (Batch)",
                OldValue = oldStatus.ToString(),
                NewValue = status.ToString(),
                DateModified = now
            });

            if (!string.IsNullOrWhiteSpace(noteText))
            {
                _context.IssueNotes.Add(new IssueNote
                {
                    IssueId = issue.Id,
                    ReporterId = currentUserId,
                    Note = noteText,
                    DateSubmitted = now,
                    LastModified = now
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchAssignAsync(IEnumerable<int> issueIds, int? handlerId, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var issue in issues)
        {
            var oldHandler = issue.HandlerId;
            issue.HandlerId = (handlerId.HasValue && handlerId.Value > 0) ? handlerId.Value : null;
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "Assigned To (Batch)",
                OldValue = oldHandler?.ToString() ?? "Unassigned",
                NewValue = issue.HandlerId?.ToString() ?? "Unassigned",
                DateModified = now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchChangePriorityAsync(IEnumerable<int> issueIds, IssuePriority priority, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var issue in issues)
        {
            var oldPriority = issue.Priority;
            issue.Priority = priority;
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "Priority (Batch)",
                OldValue = oldPriority.ToString(),
                NewValue = priority.ToString(),
                DateModified = now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchChangeSeverityAsync(IEnumerable<int> issueIds, IssueSeverity severity, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var issue in issues)
        {
            var oldSeverity = issue.Severity;
            issue.Severity = severity;
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "Severity (Batch)",
                OldValue = oldSeverity.ToString(),
                NewValue = severity.ToString(),
                DateModified = now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchChangeViewStateAsync(IEnumerable<int> issueIds, ProjectViewState viewState, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var issue in issues)
        {
            var oldState = issue.ViewState;
            issue.ViewState = viewState;
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "View Status (Batch)",
                OldValue = oldState.ToString(),
                NewValue = viewState.ToString(),
                DateModified = now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchMoveProjectAsync(IEnumerable<int> issueIds, int targetProjectId, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        var targetProject = await _context.Projects.FindAsync(targetProjectId);
        if (targetProject == null) return false;

        var now = DateTime.UtcNow;
        foreach (var issue in issues)
        {
            var oldProjId = issue.ProjectId;
            issue.ProjectId = targetProjectId;
            issue.LastUpdated = now;

            _context.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                FieldName = "Project (Batch Move)",
                OldValue = $"Project #{oldProjId}",
                NewValue = targetProject.Name,
                DateModified = now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BatchDeleteAsync(IEnumerable<int> issueIds, int currentUserId)
    {
        var issues = await _context.Issues.Where(i => issueIds.Contains(i.Id)).ToListAsync();
        _context.Issues.RemoveRange(issues);
        await _context.SaveChangesAsync();
        return true;
    }
}
