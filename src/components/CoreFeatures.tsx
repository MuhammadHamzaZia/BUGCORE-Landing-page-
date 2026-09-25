import React, { useState } from 'react';
import { CORE_FEATURES_LIST } from '../data/mockData';
import { Copy, Check } from 'lucide-react';

export const CoreFeatures: React.FC = () => {
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const toggleExpand = (id: string) => {
    setExpandedId((curr) => (curr === id ? null : id));
  };

  const handleCopyCode = (id: string, code: string) => {
    navigator.clipboard.writeText(code);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  return (
    <section id="features" className="bg-white py-[clamp(3rem,6vw,5.5rem)] border-t border-b border-gray-200">
      <div className="mx-auto w-full max-w-[min(100%-1.5rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] px-3 sm:px-6 lg:px-8">
        {/* Wireframe Title: CORE FEATURES */}
        <div className="mb-8 sm:mb-12">
          <h2 className="text-[clamp(1.35rem,2.4vw,1.85rem)] font-extrabold tracking-tight text-gray-950 font-mono">
            CORE FEATURES
          </h2>
        </div>

        {/* Fluid 2-column Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 lg:gap-8 xl:gap-10 items-start">
          {CORE_FEATURES_LIST.map((feature) => {
            const isExpanded = expandedId === feature.id;
            return (
              <div
                key={feature.id}
                className="border border-gray-300 bg-white p-5 sm:p-6 rounded-xs hover:border-gray-500 transition-colors flex flex-col justify-between shadow-xs"
              >
                <div>
                  <div className="flex flex-wrap items-center justify-between gap-2 mb-3">
                    <h3 className="text-base sm:text-lg font-bold text-gray-950 font-mono tracking-tight">
                      [ {feature.title} ]
                    </h3>
                    <button
                      onClick={() => toggleExpand(feature.id)}
                      className="text-xs font-mono font-semibold text-gray-650 text-gray-600 hover:text-black underline decoration-gray-400 hover:decoration-black cursor-pointer py-0.5"
                    >
                      {isExpanded ? '[ hide code ]' : '[ view C# code ]'}
                    </button>
                  </div>

                  <p className="text-sm sm:text-base font-medium text-gray-700 leading-relaxed font-sans">
                    {feature.description}
                  </p>
                </div>

                {/* Optional expandable implementation code block */}
                {isExpanded && (
                  <div className="mt-4 border border-gray-300 bg-gray-50 p-4 rounded-xs">
                    <div className="flex items-center justify-between border-b border-gray-200 pb-2 mb-3">
                      <span className="font-mono text-xs font-bold text-gray-600">
                        ASP.NET Core / EF Core Snippet
                      </span>
                      <button
                        onClick={() => handleCopyCode(feature.id, feature.codeSnippet)}
                        className="inline-flex items-center gap-1.5 text-xs font-mono font-bold text-gray-800 hover:text-black border border-gray-300 bg-white px-2.5 py-1 rounded-xs cursor-pointer shadow-2xs"
                      >
                        {copiedId === feature.id ? (
                          <>
                            <Check className="w-3.5 h-3.5 text-emerald-600 stroke-[2.5]" />
                            <span className="text-emerald-700 font-bold">Copied</span>
                          </>
                        ) : (
                          <>
                            <Copy className="w-3.5 h-3.5 text-gray-600 stroke-[2.2]" />
                            <span>Copy</span>
                          </>
                        )}
                      </button>
                    </div>

                    <pre className="font-mono text-xs text-gray-950 font-semibold overflow-x-auto leading-relaxed whitespace-pre selection:bg-gray-200 max-h-[300px] overscroll-contain">
                      <code>{feature.codeSnippet}</code>
                    </pre>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
};
