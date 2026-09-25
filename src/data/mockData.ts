export interface Issue {
  id: string;
  key: string;
  title: string;
  status: 'NEW' | 'ACKNOWLEDGED' | 'CONFIRMED' | 'ASSIGNED' | 'RESOLVED' | 'CLOSED';
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  priority: 'Immediate' | 'Urgent' | 'Normal' | 'Low';
  project: string;
  assignee: string;
  reporter: string;
  updatedAt: string;
  slackSynced: boolean;
  slackThreadCount: number;
  customAttributes: Record<string, string>;
  history: Array<{
    timestamp: string;
    actor: string;
    action: string;
    note?: string;
  }>;
}

export const INITIAL_DEFECT: Issue = {
  id: 'bc-1048',
  key: 'BUG-1048',
  title: 'Connection pool exhaustion under high concurrency on Npgsql 8.0.3',
  status: 'ASSIGNED',
  severity: 'High',
  priority: 'Urgent',
  project: 'Payments / Settlement Engine',
  assignee: 'sarah.chen@eng',
  reporter: 'marcus.vance@qa',
  updatedAt: '2 mins ago',
  slackSynced: true,
  slackThreadCount: 4,
  customAttributes: {
    'Runtime Environment': 'Linux x64 (.NET 9.0)',
    'Regression Commit': 'c8f421a9',
    'Customer Tier': 'Enterprise S-1',
  },
  history: [
    {
      timestamp: '14:22 UTC',
      actor: 'marcus.vance@qa',
      action: 'Created defect #1048 with status NEW',
      note: 'Observed intermittent timeout during 10k batch charge load test.',
    },
    {
      timestamp: '14:25 UTC',
      actor: 'system (EF Core)',
      action: 'State moved from NEW to ACKNOWLEDGED',
      note: 'Automated triage rules applied based on project severity matrix.',
    },
    {
      timestamp: '14:31 UTC',
      actor: 'alex.rivera@lead',
      action: 'State moved from ACKNOWLEDGED to CONFIRMED',
      note: 'Reproduced locally with Docker container postgres:16-alpine.',
    },
    {
      timestamp: '14:38 UTC',
      actor: 'sarah.chen@eng',
      action: 'State moved from CONFIRMED to ASSIGNED (via Slack /bug claim)',
      note: 'Claimed ticket directly in #eng-settlement Slack thread.',
    },
  ],
};

export const STATUS_SEQUENCE: Array<Issue['status']> = [
  'NEW',
  'ACKNOWLEDGED',
  'CONFIRMED',
  'ASSIGNED',
  'RESOLVED',
  'CLOSED',
];

export const CORE_FEATURES_LIST = [
  {
    id: 'state-machine',
    title: 'Relational State Machine',
    tag: 'EF Core 9.0',
    description:
      'Issues move linearly through statuses with full audit logging via Entity Framework Core.',
    details:
      'Guarantees strict lifecycle compliance at the database level. Every transition triggers an immutable AuditRecord row with timestamp, previous state, target state, actor identity, and change delta.',
    codeSnippet: `// BugStateMachine.cs - Strict linear progression
builder.Entity<Issue>()
    .HasOne(i => i.CurrentState)
    .WithMany()
    .HasForeignKey(i => i.StatusId);

public async Task<TransitionResult> TransitionAsync(
    int issueId, IssueStatus targetStatus, string actor)
{
    var issue = await _db.Issues.FindAsync(issueId);
    if (!AllowedTransitions[issue.Status].Contains(targetStatus))
        throw new InvalidStateTransitionException(issue.Status, targetStatus);

    var audit = new AuditLog {
        IssueId = issue.Id,
        FromState = issue.Status,
        ToState = targetStatus,
        Actor = actor,
        TimestampUtc = DateTimeOffset.UtcNow
    };
    
    issue.Status = targetStatus;
    _db.AuditLogs.Add(audit);
    await _db.SaveChangesAsync();
    return TransitionResult.Success();
}`,
  },
  {
    id: 'chatops',
    title: 'Bidirectional ChatOps',
    tag: 'Slack & Teams',
    description:
      'Claim tickets, resolve bugs, and sync thread comments directly from Slack and Microsoft Teams.',
    details:
      'Zero manual copy-pasting. Incoming webhooks verify HMAC signatures, parse slash commands (/bug claim, /bug status), and broadcast real-time state changes with interactive Slack Block Kit actions.',
    codeSnippet: `// SlackWebhookController.cs
[HttpPost("api/v1/slack/commands")]
[ValidateSlackSignature]
public async Task<IActionResult> HandleSlashCommand([FromForm] SlackCommand cmd)
{
    if (cmd.Text.StartsWith("claim"))
    {
        var issueId = ParseIssueId(cmd.Text);
        await _stateMachine.TransitionAsync(issueId, IssueStatus.Assigned, cmd.UserName);
        
        await _slackClient.PostThreadReplyAsync(cmd.ChannelId, cmd.ThreadTs, 
            $"Ticket #{issueId} claimed by @{cmd.UserName}. State updated to ASSIGNED.");
        return Ok(new SlackEphemeralResponse("Claim registered."));
    }
    return BadRequest("Unknown command");
}`,
  },
  {
    id: 'custom-attributes',
    title: 'Dynamic Custom Attributes',
    tag: 'Hybrid EAV / JSONB',
    description:
      'Map unique fields to specific projects without altering the database schema.',
    details:
      'Projects specify dynamic fields (e.g. Firmware Revision, Target Milestone, Customer Tier) mapped directly to a indexed PostgreSQL JSONB column or EF Core shadow properties without requiring migrations.',
    codeSnippet: `// ProjectAttributeConfiguration.cs
public class ProjectCustomField
{
    public string FieldKey { get; set; } = "";
    public FieldType Type { get; set; } // Text, Number, Dropdown, SemVer
    public bool IsRequired { get; set; }
    public string[]? AllowedOptions { get; set; }
}

// EF Core Model Mapping to JSONB
modelBuilder.Entity<Issue>()
    .Property(b => b.CustomAttributes)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new()
    );`,
  },
  {
    id: 'rbac',
    title: 'Role-Based Access Control',
    tag: 'Per-Project Granular',
    description:
      'Override global user roles on a per-project basis for strict security.',
    details:
      'Fine-grained permissions for multi-tenant organizations. A developer globally can be restricted to Read-Only or External Contributor in sensitive repositories like Billing or Cryptography.',
    codeSnippet: `// RbacAuthorizationHandler.cs
public async Task<bool> AuthorizeAsync(
    ClaimsPrincipal user, int projectId, Permission requiredPermission)
{
    var userId = user.GetUserId();
    var projectRole = await _db.ProjectMemberships
        .Where(m => m.ProjectId == projectId && m.UserId == userId)
        .Select(m => m.ProjectRoleOverride)
        .FirstOrDefaultAsync();

    var effectiveRole = projectRole ?? user.GetGlobalRole();
    return RolePermissionMatrix[effectiveRole].Contains(requiredPermission);
}`,
  },
];

export const DEPLOY_OPTIONS = {
  dotnet: {
    label: '.NET CLI (Native)',
    commands: [
      '$ git clone https://github.com/MuhammadHamzaZia/BUGCORE.git',
      '$ cd bugcore/src/bugcore.web',
      '$ dotnet run --environment Production',
    ],
    note: 'Requires .NET 9.0 SDK or ASP.NET Core Runtime. Uses built-in SQLite by default; configure PostgreSQL via appsettings.json or environment variables.',
  },
  docker: {
    label: 'Docker Compose',
    commands: [
      '$ curl -O https://raw.githubusercontent.com/MuhammadHamzaZia/BUGCORE/main/docker-compose.yml',
      '$ docker compose up -d',
      '# Listening on http://localhost:5000',
    ],
    note: 'Spins up BugCore Web container, PostgreSQL 16 database, and Slack webhook proxy with automatic health checks.',
  },
  binary: {
    label: 'Single-File Native AOT',
    commands: [
      '$ curl -L https://github.com/MuhammadHamzaZia/BUGCORE/releases/download/v2.4.1/bugcore-linux-x64.tar.gz | tar -xz',
      '$ ./bugcore --urls "http://0.0.0.0:8080"',
      '# Zero dependencies. Native machine code binary (~38MB).',
    ],
    note: 'Self-contained executable compiled with .NET Native AOT. Instant startup in under 45ms with minimal RAM consumption.',
  },
};

export const API_ENDPOINTS = [
  {
    method: 'GET',
    path: '/api/v1/issues',
    desc: 'Paginated list of issues with filtering by project, status, and severity.',
  },
  {
    method: 'POST',
    path: '/api/v1/issues',
    desc: 'Create a new defect record with validated schema and custom project attributes.',
  },
  {
    method: 'POST',
    path: '/api/v1/issues/{id}/transition',
    desc: 'Execute a linear state transition; verifies RBAC permissions and appends to EF audit log.',
  },
  {
    method: 'POST',
    path: '/api/v1/webhooks/slack/events',
    desc: 'Signed Slack event endpoint for bidirectional slash commands and thread sync.',
  },
  {
    method: 'GET',
    path: '/api/v1/projects/{id}/rbac',
    desc: 'Inspect granular role overrides and user permissions for a specific project.',
  },
];
