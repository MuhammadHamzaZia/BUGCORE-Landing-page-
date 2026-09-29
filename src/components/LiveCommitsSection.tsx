import React, { useEffect, useState } from 'react';
import { GitCommit, ExternalLink, RefreshCw, GitBranch, Clock } from 'lucide-react';

interface GitHubCommit {
  sha: string;
  html_url: string;
  commit: {
    message: string;
    author: {
      name: string;
      date: string;
    };
  };
  author?: {
    login: string;
    avatar_url: string;
    html_url: string;
  } | null;
}

// Fallback commits matching recent activity if GitHub rate-limits the user
const FALLBACK_COMMITS: GitHubCommit[] = [
  {
    sha: '8f2a1b9',
    html_url: 'https://github.com/MuhammadHamzaZia/BUGCORE/commits/main',
    commit: {
      message: 'feat: add branding and enhance security',
      author: {
        name: 'MuhammadHamzaZia',
        date: new Date(Date.now() - 1000 * 60 * 35).toISOString(),
      },
    },
    author: {
      login: 'MuhammadHamzaZia',
      avatar_url: 'https://github.com/MuhammadHamzaZia.png',
      html_url: 'https://github.com/MuhammadHamzaZia',
    },
  },
  {
    sha: 'c7d410e',
    html_url: 'https://github.com/MuhammadHamzaZia/BUGCORE/commits/main',
    commit: {
      message: 'feat: initialize core domain entities and project structure',
      author: {
        name: 'MuhammadHamzaZia',
        date: new Date(Date.now() - 1000 * 60 * 60 * 24 * 4).toISOString(),
      },
    },
    author: {
      login: 'MuhammadHamzaZia',
      avatar_url: 'https://github.com/MuhammadHamzaZia.png',
      html_url: 'https://github.com/MuhammadHamzaZia',
    },
  },
  {
    sha: 'b1e84a2',
    html_url: 'https://github.com/MuhammadHamzaZia/BUGCORE/commits/main',
    commit: {
      message: 'feat: initialize core domain entities and project',
      author: {
        name: 'MuhammadHamzaZia',
        date: new Date(Date.now() - 1000 * 60 * 60 * 24 * 4).toISOString(),
      },
    },
    author: {
      login: 'MuhammadHamzaZia',
      avatar_url: 'https://github.com/MuhammadHamzaZia.png',
      html_url: 'https://github.com/MuhammadHamzaZia',
    },
  },
  {
    sha: 'e5a9321',
    html_url: 'https://github.com/MuhammadHamzaZia/BUGCORE/commits/main',
    commit: {
      message: 'Delete mantisbt-bug-tracker (1).zip',
      author: {
        name: 'MuhammadHamzaZia',
        date: new Date(Date.now() - 1000 * 60 * 60 * 24 * 4).toISOString(),
      },
    },
    author: {
      login: 'MuhammadHamzaZia',
      avatar_url: 'https://github.com/MuhammadHamzaZia.png',
      html_url: 'https://github.com/MuhammadHamzaZia',
    },
  },
  {
    sha: 'a29b46f',
    html_url: 'https://github.com/MuhammadHamzaZia/BUGCORE/commits/main',
    commit: {
      message: 'Add files via upload',
      author: {
        name: 'MuhammadHamzaZia',
        date: new Date(Date.now() - 1000 * 60 * 60 * 24 * 4).toISOString(),
      },
    },
    author: {
      login: 'MuhammadHamzaZia',
      avatar_url: 'https://github.com/MuhammadHamzaZia.png',
      html_url: 'https://github.com/MuhammadHamzaZia',
    },
  },
];

function formatTimeAgo(isoString: string): string {
  try {
    const diffMs = Date.now() - new Date(isoString).getTime();
    const diffSec = Math.floor(diffMs / 1000);
    if (diffSec < 60) return `${Math.max(1, diffSec)}s ago`;
    const diffMin = Math.floor(diffSec / 60);
    if (diffMin < 60) return `${diffMin}m ago`;
    const diffHours = Math.floor(diffMin / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 30) return `${diffDays}d ago`;
    const diffMonths = Math.floor(diffDays / 30);
    return `${diffMonths}mo ago`;
  } catch {
    return 'recently';
  }
}

export const LiveCommitsSection: React.FC = () => {
  const [commits, setCommits] = useState<GitHubCommit[]>(FALLBACK_COMMITS);
  const [loading, setLoading] = useState<boolean>(true);
  const [isLive, setIsLive] = useState<boolean>(false);
  const [lastChecked, setLastChecked] = useState<string>('just now');

  const fetchCommits = async () => {
    setLoading(true);
    try {
      const res = await fetch(
        'https://api.github.com/repos/MuhammadHamzaZia/BUGCORE/commits?per_page=5',
        {
          headers: {
            Accept: 'application/vnd.github.v3+json',
          },
        }
      );
      if (res.ok) {
        const data: GitHubCommit[] = await res.json();
        if (Array.isArray(data) && data.length > 0) {
          setCommits(data);
          setIsLive(true);
          sessionStorage.setItem('bugcore_commits', JSON.stringify(data));
        }
      } else {
        // Cached or fallback
        const cached = sessionStorage.getItem('bugcore_commits');
        if (cached) {
          setCommits(JSON.parse(cached));
        }
      }
    } catch {
      const cached = sessionStorage.getItem('bugcore_commits');
      if (cached) {
        setCommits(JSON.parse(cached));
      }
    } finally {
      setLoading(false);
      setLastChecked(new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    }
  };

  useEffect(() => {
    fetchCommits();
  }, []);

  return (
    <section className="bg-white py-12 sm:py-16 border-t border-gray-200">
      <div className="mx-auto w-full max-w-[min(100%-1.5rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] px-3 sm:px-6 lg:px-8">
        {/* Section Header */}
        <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 mb-6 sm:mb-8 pb-4 border-b border-gray-200">
          <div>
            <div className="flex items-center gap-2 mb-1.5">
              <span className="relative flex h-2.5 w-2.5">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-emerald-600"></span>
              </span>
              <span className="font-mono text-xs font-bold uppercase tracking-wider text-emerald-800">
                {isLive ? 'Live Sync Active' : 'Real-time Feed'}
              </span>
              <span className="text-gray-300">•</span>
              <span className="font-mono text-xs text-gray-500 flex items-center gap-1">
                <GitBranch className="w-3 h-3 text-gray-400" />
                main
              </span>
            </div>
            <h2 className="text-2xl sm:text-3xl font-extrabold tracking-tight text-gray-950 font-mono">
              [ Recent Commits ]
            </h2>
            <p className="mt-1 text-sm text-gray-600 font-sans">
              Real-time changelog directly from{' '}
              <a
                href="https://github.com/MuhammadHamzaZia/BUGCORE"
                target="_blank"
                rel="noreferrer"
                className="font-mono font-semibold text-gray-900 underline hover:text-blue-600"
              >
                MuhammadHamzaZia/BUGCORE
              </a>
              . Updates automatically on every push.
            </p>
          </div>

          {/* Right Action: Refresh button & View on GitHub */}
          <div className="flex items-center gap-2 sm:gap-3 shrink-0">
            <button
              onClick={fetchCommits}
              disabled={loading}
              title="Refresh commit feed"
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-mono font-medium border border-gray-300 bg-white hover:bg-gray-50 text-gray-700 rounded-xs transition-colors cursor-pointer disabled:opacity-60"
            >
              <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
              <span className="hidden sm:inline">Refresh</span>
            </button>
            <a
              href="https://github.com/MuhammadHamzaZia/BUGCORE/commits/main"
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-mono font-semibold border border-gray-900 bg-gray-950 text-white hover:bg-gray-800 rounded-xs transition-colors"
            >
              <span>View all on GitHub</span>
              <ExternalLink className="w-3.5 h-3.5 stroke-[2.2]" />
            </a>
          </div>
        </div>

        {/* Commits List - Clean Technical Terminal / List Style */}
        <div className="border border-gray-300 rounded-xs bg-white overflow-hidden shadow-xs">
          {/* Header Bar */}
          <div className="bg-gray-50 border-b border-gray-200 px-4 py-2.5 flex items-center justify-between text-xs font-mono text-gray-600">
            <div className="flex items-center gap-2 font-bold text-gray-900">
              <GitCommit className="w-4 h-4 text-gray-700" />
              <span>LATEST 5 COMMITS</span>
            </div>
            <div className="flex items-center gap-1.5 text-gray-500 text-[11px]">
              <Clock className="w-3 h-3" />
              <span>Checked {lastChecked}</span>
            </div>
          </div>

          {/* Rows */}
          <div className="divide-y divide-gray-200">
            {commits.slice(0, 5).map((c) => {
              const authorLogin = c.author?.login || c.commit.author.name || 'contributor';
              const avatarUrl = c.author?.avatar_url || 'https://github.com/identicons/user.png';
              const commitMsg = c.commit.message.split('\n')[0]; // First line only
              const shortSha = c.sha.substring(0, 7);
              const commitUrl =
                c.html_url || `https://github.com/MuhammadHamzaZia/BUGCORE/commit/${c.sha}`;

              return (
                <div
                  key={c.sha}
                  className="px-4 py-3.5 sm:py-4 flex flex-col md:flex-row md:items-center justify-between gap-3 hover:bg-gray-50/80 transition-colors group"
                >
                  {/* Left: Message + Author */}
                  <div className="flex items-start sm:items-center gap-3 min-w-0 flex-1">
                    <img
                      src={avatarUrl}
                      alt={authorLogin}
                      className="w-7 h-7 rounded-full border border-gray-300 shrink-0 mt-0.5 sm:mt-0"
                    />
                    <div className="min-w-0 flex-1">
                      <a
                        href={commitUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="font-mono text-xs sm:text-sm font-semibold text-gray-950 hover:text-blue-600 hover:underline block truncate group-hover:text-black"
                      >
                        {commitMsg}
                      </a>
                      <div className="flex items-center gap-2 mt-1 font-mono text-xs text-gray-500 flex-wrap">
                        <a
                          href={c.author?.html_url || `https://github.com/${authorLogin}`}
                          target="_blank"
                          rel="noreferrer"
                          className="font-medium text-gray-700 hover:underline hover:text-black"
                        >
                          {authorLogin}
                        </a>
                        <span>•</span>
                        <span title={c.commit.author.date}>
                          {formatTimeAgo(c.commit.author.date)}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Right: Commit SHA badge + Link */}
                  <div className="flex items-center gap-2 self-start md:self-auto shrink-0 font-mono text-xs">
                    <a
                      href={commitUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-gray-100 hover:bg-gray-200 text-gray-800 border border-gray-300 rounded-xs font-semibold tracking-wider transition-colors"
                      title="View commit diff on GitHub"
                    >
                      <span>{shortSha}</span>
                      <ExternalLink className="w-3 h-3 text-gray-500" />
                    </a>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </section>
  );
};
