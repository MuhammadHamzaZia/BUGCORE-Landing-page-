import React, { useState } from 'react';
import { X, BookOpen, Terminal, Shield, Check, Copy } from 'lucide-react';

interface DocModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const DocModal: React.FC<DocModalProps> = ({ isOpen, onClose }) => {
  const [activeSection, setActiveSection] = useState<'quickstart' | 'config' | 'rbac'>('quickstart');
  const [copied, setCopied] = useState(false);

  if (!isOpen) return null;

  const appsettingsContent = `{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bugcore_prod;Username=bugcore;Password=secret"
  },
  "BugCore": {
    "InstanceName": "Internal Defect Tracker",
    "AllowSelfRegistration": false,
    "DefaultProjectRole": "Reporter",
    "Slack": {
      "Enabled": true,
      "SigningSecret": "env:SLACK_SIGNING_SECRET",
      "BotToken": "env:SLACK_BOT_TOKEN",
      "DefaultChannel": "#eng-defects"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 bg-gray-900/60 backdrop-blur-none">
      <div className="w-full max-w-[min(94vw,900px)] max-h-[92vh] flex flex-col bg-white border border-gray-300 rounded-xs shadow-xl">
        {/* Modal Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-3 sm:px-6 py-3 sm:py-4 bg-gray-50">
          <div className="flex items-center gap-2 sm:gap-2.5">
            <img src="/logo.svg" alt="BUGCORE" className="w-5 h-5 object-contain" />
            <h3 className="font-mono text-sm sm:text-base font-bold text-gray-900">
              BUGCORE DOCUMENTATION
            </h3>
            <span className="hidden sm:inline font-mono text-xs text-gray-500 bg-white border border-gray-200 px-1.5 py-0.5">
              v2.4.1 Manual
            </span>
          </div>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-900 border border-gray-200 hover:border-gray-400 p-1.5 bg-white cursor-pointer rounded-xs"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Modal Navigation Tabs */}
        <div className="flex border-b border-gray-200 px-3 sm:px-6 bg-white font-mono text-xs overflow-x-auto whitespace-nowrap">
          <button
            onClick={() => setActiveSection('quickstart')}
            className={`py-2.5 px-3 border-b-2 font-medium cursor-pointer ${
              activeSection === 'quickstart'
                ? 'border-gray-900 text-gray-900 font-bold'
                : 'border-transparent text-gray-600 hover:text-gray-900'
            }`}
          >
            1. Quick Start
          </button>
          <button
            onClick={() => setActiveSection('config')}
            className={`py-2.5 px-3 border-b-2 font-medium cursor-pointer ${
              activeSection === 'config'
                ? 'border-gray-900 text-gray-900 font-bold'
                : 'border-transparent text-gray-600 hover:text-gray-900'
            }`}
          >
            2. appsettings.json
          </button>
          <button
            onClick={() => setActiveSection('rbac')}
            className={`py-2.5 px-3 border-b-2 font-medium cursor-pointer ${
              activeSection === 'rbac'
                ? 'border-gray-900 text-gray-900 font-bold'
                : 'border-transparent text-gray-600 hover:text-gray-900'
            }`}
          >
            3. RBAC & Security
          </button>
        </div>

        {/* Modal Body */}
        <div className="p-6 overflow-y-auto space-y-5 text-sm text-gray-700 leading-relaxed font-sans">
          {activeSection === 'quickstart' && (
            <div className="space-y-4">
              <h4 className="text-base font-bold text-gray-900 font-mono">
                System Requirements & First Run
              </h4>
              <p>
                BugCore requires either the .NET 9.0 SDK installed on the host machine or
                Docker Engine. When started without a configured connection string, BugCore
                automatically provisions a local SQLite database in <code>data/bugcore.db</code>.
              </p>

              <div className="border border-gray-200 bg-gray-50 p-3 font-mono text-xs">
                <div className="text-gray-500 mb-1"># Clone and build locally</div>
                <div className="text-gray-900">git clone https://github.com/MuhammadHamzaZia/BUGCORE.git</div>
                <div className="text-gray-900">cd bugcore/src/bugcore.web</div>
                <div className="text-gray-900">dotnet run</div>
              </div>

              <div className="border-l-2 border-gray-900 pl-4 py-1 text-xs text-gray-600">
                <strong>Initial Credentials:</strong> On first execution, an initial administrator account
                is seeded with username <code>admin@local</code> and a temporary token printed to stdout.
              </div>
            </div>
          )}

          {activeSection === 'config' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h4 className="text-base font-bold text-gray-900 font-mono">
                  Production appsettings.json
                </h4>
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(appsettingsContent);
                    setCopied(true);
                    setTimeout(() => setCopied(false), 2000);
                  }}
                  className="inline-flex items-center gap-1 text-xs font-mono border border-gray-300 px-2 py-1 bg-white hover:bg-gray-50 cursor-pointer"
                >
                  {copied ? <Check className="w-3 h-3 text-emerald-600" /> : <Copy className="w-3 h-3 text-gray-500" />}
                  <span>{copied ? 'Copied' : 'Copy JSON'}</span>
                </button>
              </div>
              <p className="text-xs text-gray-600">
                Supports environment variable substitution via standard ASP.NET Core configuration providers
                (e.g., <code>ConnectionStrings__DefaultConnection</code>).
              </p>

              <pre className="border border-gray-200 bg-gray-50 p-3 text-xs font-mono text-gray-900 overflow-x-auto">
                <code>{appsettingsContent}</code>
              </pre>
            </div>
          )}

          {activeSection === 'rbac' && (
            <div className="space-y-4">
              <h4 className="text-base font-bold text-gray-900 font-mono">
                Granular Role-Based Access Control
              </h4>
              <p>
                BugCore defines four base system roles. Each user maintains a global role that can be
                overridden per repository or project:
              </p>
              <div className="border border-gray-200 divide-y divide-gray-200 text-xs font-mono">
                <div className="p-2.5 bg-gray-50">
                  <strong className="text-gray-900">Viewer / Guest:</strong> Read-only access to published defects. Cannot comment or transition states.
                </div>
                <div className="p-2.5 bg-white">
                  <strong className="text-gray-900">Reporter:</strong> Can create defects, upload logs/screenshots, and comment on reported issues.
                </div>
                <div className="p-2.5 bg-gray-50">
                  <strong className="text-gray-900">Developer:</strong> Can claim tickets, advance statuses (Assign, Resolve), and link git commits.
                </div>
                <div className="p-2.5 bg-white">
                  <strong className="text-gray-900">Project Manager / Admin:</strong> Can customize dynamic attributes, close bugs, and override project permissions.
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="border-t border-gray-200 px-6 py-3 bg-gray-50 flex items-center justify-between text-xs font-mono text-gray-500">
          <span>Documentation licensed under CC BY 4.0</span>
          <button
            onClick={onClose}
            className="border border-gray-300 bg-white hover:bg-gray-100 text-gray-900 px-4 py-1.5 rounded-sm font-medium cursor-pointer"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
