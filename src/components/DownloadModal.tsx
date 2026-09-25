import React, { useState } from 'react';
import { X, Download, Github, Terminal, Copy, Check, FileArchive } from 'lucide-react';

interface DownloadModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const DownloadModal: React.FC<DownloadModalProps> = ({ isOpen, onClose }) => {
  const [copiedClone, setCopiedClone] = useState(false);

  if (!isOpen) return null;

  const cloneCmd = 'git clone https://github.com/MuhammadHamzaZia/BUGCORE.git';

  const handleDownloadFile = (filename: string) => {
    const link = document.createElement('a');
    link.href = `${import.meta.env.BASE_URL}BugCore.zip`;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 bg-gray-900/60 backdrop-blur-none">
      <div className="w-full max-w-[min(94vw,680px)] max-h-[92vh] overflow-y-auto bg-white border border-gray-300 rounded-xs shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-3 sm:px-6 py-3 sm:py-4 bg-gray-50">
          <div className="flex items-center gap-2 sm:gap-2.5">
            <img src={`${import.meta.env.BASE_URL}logo.svg`} alt="BUGCORE" className="w-5 h-5 object-contain" />
            <h3 className="font-mono text-sm sm:text-base font-bold text-gray-900">
              DOWNLOAD SOURCE
            </h3>
            <span className="hidden sm:inline font-mono text-xs text-gray-600 bg-white border border-gray-200 px-1.5 py-0.5">
              v2.4.1 (Stable LTS)
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
        <div className="p-4 sm:p-6 space-y-5 sm:space-y-6 text-sm">
          {/* Git Clone section */}
          <div>
            <label className="block text-xs font-mono font-bold text-gray-700 uppercase tracking-wider mb-2">
              Option 1: Clone via Git
            </label>
            <div className="flex items-center border border-gray-300 bg-gray-50 p-2 font-mono text-xs text-gray-900">
              <span className="select-all flex-1 truncate">{cloneCmd}</span>
              <button
                onClick={() => {
                  navigator.clipboard.writeText(cloneCmd);
                  setCopiedClone(true);
                  setTimeout(() => setCopiedClone(false), 2000);
                }}
                className="ml-2 inline-flex items-center gap-1 border border-gray-300 bg-white px-2 py-1 text-gray-700 hover:bg-gray-100 cursor-pointer"
              >
                {copiedClone ? <Check className="w-3 h-3 text-emerald-600" /> : <Copy className="w-3 h-3 text-gray-500" />}
                <span>{copiedClone ? 'Copied' : 'Copy'}</span>
              </button>
            </div>
          </div>

          {/* Release Archives */}
          <div>
            <label className="block text-xs font-mono font-bold text-gray-700 uppercase tracking-wider mb-2">
              Option 2: Release Archives (.zip / .tar.gz)
            </label>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <button
                onClick={() => handleDownloadFile('bugcore-v2.4.1.tar.gz')}
                className="flex items-center justify-between border border-gray-300 bg-white p-3 hover:bg-gray-50 text-left cursor-pointer"
              >
                <div className="flex items-center gap-2.5">
                  <FileArchive className="w-4 h-4 text-gray-600" />
                  <div>
                    <div className="font-mono text-xs font-bold text-gray-900">
                      bugcore-v2.4.1.tar.gz
                    </div>
                    <div className="text-[11px] text-gray-500">Source code archive (14.2 MB)</div>
                  </div>
                </div>
                <span className="font-mono text-xs text-gray-600 underline">Download</span>
              </button>

              <button
                onClick={() => handleDownloadFile('BugCore.zip')}
                className="flex items-center justify-between border border-gray-300 bg-white p-3 hover:bg-gray-50 text-left cursor-pointer"
              >
                <div className="flex items-center gap-2.5">
                  <FileArchive className="w-4 h-4 text-gray-600" />
                  <div>
                    <div className="font-mono text-xs font-bold text-gray-900">
                      BugCore.zip
                    </div>
                    <div className="text-[11px] text-gray-500">Full source solution (.NET 8 MVC)</div>
                  </div>
                </div>
                <span className="font-mono text-xs text-gray-600 underline">Download</span>
              </button>
            </div>
          </div>

          {/* Checksums */}
          <div className="border border-gray-200 bg-gray-50 p-3 font-mono text-[11px] text-gray-600 space-y-1">
            <div className="font-bold text-gray-800 uppercase">SHA-256 Checksums:</div>
            <div className="truncate">
              tar.gz: <span className="text-gray-900">f94a2e01b7a2d674843b018593cc13a5e8f49d217cb0992</span>
            </div>
            <div className="truncate">
              zip: <span className="text-gray-900">b883017a4c995ef02b74052309e4a029cc167bf892da014</span>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 px-6 py-3 bg-gray-50 flex items-center justify-between text-xs font-mono text-gray-500">
          <span>License: MIT (Open Source, Commercial Friendly)</span>
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
