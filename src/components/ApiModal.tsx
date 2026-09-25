import React, { useState } from 'react';
import { X, Code2, Copy, Check } from 'lucide-react';
import { API_ENDPOINTS } from '../data/mockData';

interface ApiModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const ApiModal: React.FC<ApiModalProps> = ({ isOpen, onClose }) => {
  const [selectedEndpoint, setSelectedEndpoint] = useState(API_ENDPOINTS[0]);
  const [copied, setCopied] = useState(false);

  if (!isOpen) return null;

  const samplePayload = `{
  "id": 1048,
  "projectKey": "PAY",
  "title": "Connection pool exhaustion under high concurrency",
  "currentState": "ASSIGNED",
  "severity": "High",
  "assignee": "sarah.chen@eng",
  "customAttributes": {
    "runtime": "Linux x64 .NET 9.0",
    "regressionCommit": "c8f421a9"
  },
  "auditCount": 4,
  "createdAt": "2026-09-25T14:22:00Z"
}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 bg-gray-900/60 backdrop-blur-none">
      <div className="w-full max-w-[min(94vw,960px)] max-h-[92vh] flex flex-col bg-white border border-gray-300 rounded-xs shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-3 sm:px-6 py-3 sm:py-4 bg-gray-50">
          <div className="flex items-center gap-2 sm:gap-2.5">
            <img src={`${import.meta.env.BASE_URL}logo.svg`} alt="BUGCORE" className="w-5 h-5 object-contain" />
            <h3 className="font-mono text-sm sm:text-base font-bold text-gray-900">
              BUGCORE REST API
            </h3>
            <span className="hidden sm:inline font-mono text-xs text-gray-500 bg-white border border-gray-200 px-1.5 py-0.5">
              OpenAPI 3.1
            </span>
          </div>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-900 border border-gray-200 hover:border-gray-400 p-1.5 bg-white cursor-pointer rounded-xs"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Content */}
        <div className="grid grid-cols-1 md:grid-cols-12 flex-1 overflow-hidden">
          {/* Endpoints Sidebar */}
          <div className="md:col-span-5 border-r border-gray-200 bg-gray-50 p-4 overflow-y-auto space-y-1.5 font-mono text-xs">
            <div className="text-[11px] font-bold text-gray-500 uppercase tracking-wider mb-2">
              Endpoints Catalog
            </div>
            {API_ENDPOINTS.map((ep, idx) => {
              const isSelected = selectedEndpoint.path === ep.path && selectedEndpoint.method === ep.method;
              return (
                <button
                  key={idx}
                  onClick={() => setSelectedEndpoint(ep)}
                  className={`w-full text-left p-2 border transition-colors cursor-pointer block ${
                    isSelected
                      ? 'border-gray-900 bg-white font-bold text-gray-900'
                      : 'border-transparent text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  <div className="flex items-center gap-2">
                    <span
                      className={`px-1 py-0.5 text-[10px] font-bold ${
                        ep.method === 'GET'
                          ? 'bg-blue-50 text-blue-700 border border-blue-200'
                          : 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                      }`}
                    >
                      {ep.method}
                    </span>
                    <span className="truncate">{ep.path}</span>
                  </div>
                </button>
              );
            })}
          </div>

          {/* Endpoint Details */}
          <div className="md:col-span-7 p-6 overflow-y-auto space-y-4">
            <div>
              <div className="flex items-center gap-2 font-mono text-xs">
                <span className="font-bold text-gray-900 bg-gray-100 border border-gray-200 px-1.5 py-0.5">
                  {selectedEndpoint.method}
                </span>
                <span className="font-mono text-gray-800">{selectedEndpoint.path}</span>
              </div>
              <p className="mt-2 text-sm text-gray-600">{selectedEndpoint.desc}</p>
            </div>

            <div>
              <div className="flex items-center justify-between text-xs font-mono text-gray-500 mb-1.5">
                <span>Sample JSON Response (200 OK):</span>
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(samplePayload);
                    setCopied(true);
                    setTimeout(() => setCopied(false), 2000);
                  }}
                  className="inline-flex items-center gap-1 text-[11px] border border-gray-200 bg-white px-2 py-0.5 hover:bg-gray-50 cursor-pointer"
                >
                  {copied ? <Check className="w-3 h-3 text-emerald-600" /> : <Copy className="w-3 h-3 text-gray-500" />}
                  <span>{copied ? 'Copied' : 'Copy'}</span>
                </button>
              </div>
              <pre className="border border-gray-200 bg-gray-50 p-3 font-mono text-xs text-gray-900 overflow-x-auto">
                <code>{samplePayload}</code>
              </pre>
            </div>

            <div className="text-xs font-mono text-gray-500 border-t border-gray-200 pt-3">
              Authentication: Bearer Token or Session Cookie. Rate limit: 2,000 req/min.
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 px-6 py-3 bg-gray-50 flex items-center justify-between text-xs font-mono text-gray-500">
          <span>Swagger UI available at /swagger when in Development environment</span>
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
