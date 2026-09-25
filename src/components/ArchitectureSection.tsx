import React from 'react';
import { Layers, Database, Lock, Terminal, Shield, Check, X } from 'lucide-react';

export const ArchitectureSection: React.FC = () => {
  return (
    <section id="architecture" className="bg-white py-14 sm:py-20 border-b border-gray-200">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mb-10">
          <div className="font-mono text-xs uppercase tracking-wider text-gray-500 mb-2">
            System Architecture & Comparison
          </div>
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight text-gray-900 font-mono">
            WHY BUGCORE?
          </h2>
          <p className="mt-3 text-base text-gray-600">
            Built for engineering organizations that require strict internal data ownership,
            rapid page load times (&lt;40ms), and predictable relational consistency.
          </p>
        </div>

        {/* Comparison Table */}
        <div className="border border-gray-200 bg-white overflow-x-auto mb-12">
          <table className="w-full text-left border-collapse text-xs sm:text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50 font-mono text-gray-700">
                <th className="py-3 px-4 font-semibold">Capability</th>
                <th className="py-3 px-4 font-bold text-gray-900 bg-gray-100/60 border-x border-gray-200">
                  BUGCORE (ASP.NET Core)
                </th>
                <th className="py-3 px-4 font-semibold text-gray-600">Jira Server/DC</th>
                <th className="py-3 px-4 font-semibold text-gray-600">Linear (Cloud)</th>
                <th className="py-3 px-4 font-semibold text-gray-600">Bugzilla / Trac</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 text-gray-800">
              <tr>
                <td className="py-3 px-4 font-medium">Self-Hosted / Air-gapped</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="inline-flex items-center gap-1.5 text-gray-900">
                    <Check className="w-4 h-4 text-emerald-600" /> Yes (100% On-Prem)
                  </span>
                </td>
                <td className="py-3 px-4 text-gray-600">Discontinued / Cloud-only push</td>
                <td className="py-3 px-4 text-gray-500">No (SaaS only)</td>
                <td className="py-3 px-4 text-gray-700">Yes (PHP/Perl)</td>
              </tr>
              <tr>
                <td className="py-3 px-4 font-medium">License & Source Code</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="font-mono">MIT (Full Source Open)</span>
                </td>
                <td className="py-3 px-4 text-gray-500">Proprietary ($$$/seat)</td>
                <td className="py-3 px-4 text-gray-500">Proprietary Closed</td>
                <td className="py-3 px-4 text-gray-700">GPLv2</td>
              </tr>
              <tr>
                <td className="py-3 px-4 font-medium">Memory Footprint (Baseline)</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="font-mono text-emerald-700">~45 MB RSS</span>
                </td>
                <td className="py-3 px-4 font-mono text-gray-600">4,096+ MB (JVM heap)</td>
                <td className="py-3 px-4 text-gray-500">Hosted by vendor</td>
                <td className="py-3 px-4 font-mono text-gray-600">~120 MB (PHP-FPM)</td>
              </tr>
              <tr>
                <td className="py-3 px-4 font-medium">Bidirectional Slack ChatOps</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="inline-flex items-center gap-1.5 text-gray-900">
                    <Check className="w-4 h-4 text-emerald-600" /> Native Built-in
                  </span>
                </td>
                <td className="py-3 px-4 text-gray-600">Requires 3rd-party plugin</td>
                <td className="py-3 px-4 text-gray-700">Built-in (Cloud only)</td>
                <td className="py-3 px-4 text-gray-500">Complex custom hooks</td>
              </tr>
              <tr>
                <td className="py-3 px-4 font-medium">Relational State Machine & Audit</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="inline-flex items-center gap-1.5 text-gray-900">
                    <Check className="w-4 h-4 text-emerald-600" /> Strict EF Core Audit Log
                  </span>
                </td>
                <td className="py-3 px-4 text-gray-600">Complex XML workflows</td>
                <td className="py-3 px-4 text-gray-600">Fluid / No strict DB audit</td>
                <td className="py-3 px-4 text-gray-700">Static enum transition</td>
              </tr>
              <tr>
                <td className="py-3 px-4 font-medium">Telemetry / Data Phone-Home</td>
                <td className="py-3 px-4 font-bold text-gray-900 bg-gray-50/50 border-x border-gray-200">
                  <span className="font-mono text-emerald-700">ZERO (Strictly Disabled)</span>
                </td>
                <td className="py-3 px-4 text-gray-600">Atlassian Analytics</td>
                <td className="py-3 px-4 text-gray-500">Full Cloud Analytics</td>
                <td className="py-3 px-4 text-gray-700">None</td>
              </tr>
            </tbody>
          </table>
        </div>

        {/* Clean Stack Pipeline */}
        <div className="border border-gray-200 p-5 bg-gray-50">
          <div className="text-xs font-mono font-bold uppercase text-gray-700 mb-3">
            Layered Clean Architecture
          </div>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-3 text-xs font-mono">
            <div className="border border-gray-200 bg-white p-3">
              <span className="text-gray-500 block text-[11px] mb-1">01. Web Presentation</span>
              <strong className="text-gray-900 block">bugcore.web</strong>
              <span className="text-gray-600 text-[11px] mt-1 block">
                Razor Pages, Minimal API endpoints, TagHelpers, and Static Asset Pipeline.
              </span>
            </div>
            <div className="border border-gray-200 bg-white p-3">
              <span className="text-gray-500 block text-[11px] mb-1">02. Domain Core</span>
              <strong className="text-gray-900 block">BugCore.Core</strong>
              <span className="text-gray-600 text-[11px] mt-1 block">
                Linear State Machine, Audit invariants, Custom Attribute schema validators.
              </span>
            </div>
            <div className="border border-gray-200 bg-white p-3">
              <span className="text-gray-500 block text-[11px] mb-1">03. Persistence</span>
              <strong className="text-gray-900 block">BugCore.Data (EF Core)</strong>
              <span className="text-gray-600 text-[11px] mt-1 block">
                PostgreSQL (Npgsql), SQL Server, or SQLite with compiled query caching.
              </span>
            </div>
            <div className="border border-gray-200 bg-white p-3">
              <span className="text-gray-500 block text-[11px] mb-1">04. Integrations</span>
              <strong className="text-gray-900 block">BugCore.ChatOps</strong>
              <span className="text-gray-600 text-[11px] mt-1 block">
                Slack Block Kit handlers, MS Teams Webhooks, and generic outgoing HMAC events.
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};
