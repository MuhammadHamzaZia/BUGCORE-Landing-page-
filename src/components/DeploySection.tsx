import React, { useState } from 'react';
import { Copy, Check } from 'lucide-react';

export const DeploySection: React.FC = () => {
  const [copied, setCopied] = useState(false);

  const commandLines = [
    '$ git clone https://github.com/MuhammadHamzaZia/BUGCORE.git',
    '$ cd bugcore/src/bugcore.web',
    '$ dotnet run --environment Production',
  ];

  const handleCopy = () => {
    navigator.clipboard.writeText(commandLines.join('\n'));
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <section id="deploy" className="bg-white py-[clamp(2.5rem,5vw,5rem)]">
      <div className="mx-auto w-full max-w-[min(100%-1.5rem,1440px)] 2xl:max-w-[min(100%-3rem,1640px)] px-3 sm:px-6 lg:px-8">
        {/* Wireframe Header: DEPLOY IN 2 MINUTES */}
        <div className="mb-6">
          <h2 className="text-[clamp(1.35rem,2.4vw,1.85rem)] font-extrabold tracking-tight text-gray-950 font-mono">
            DEPLOY IN 2 MINUTES
          </h2>
        </div>

        {/* Light-Mode Code Block Per Wireframe */}
        <div className="border border-gray-300 bg-gray-50 rounded-xs relative w-full max-w-5xl shadow-xs">
          <div className="flex items-center justify-between border-b border-gray-200 px-3.5 sm:px-4 py-2.5 text-xs font-mono font-bold text-gray-700 bg-gray-100/70">
            <span>Terminal (bash)</span>
            <button
              onClick={handleCopy}
              className="inline-flex items-center gap-1.5 border border-gray-300 bg-white hover:bg-gray-50 px-2.5 py-1 text-xs font-mono font-bold text-gray-900 rounded-xs transition-colors cursor-pointer shadow-2xs"
            >
              {copied ? (
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

          <div className="p-4 sm:p-6 overflow-x-auto overscroll-contain">
            <pre className="font-mono text-xs sm:text-sm text-gray-950 font-semibold leading-relaxed space-y-2">
              {commandLines.map((line, idx) => (
                <div key={idx} className="flex">
                  <span className="select-all">{line}</span>
                </div>
              ))}
            </pre>
          </div>
        </div>
      </div>
    </section>
  );
};
