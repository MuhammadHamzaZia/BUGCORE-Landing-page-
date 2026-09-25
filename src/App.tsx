/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { Navbar } from './components/Navbar';
import { HeroSection } from './components/HeroSection';
import { CoreFeatures } from './components/CoreFeatures';
import { DeploySection } from './components/DeploySection';
import { Footer } from './components/Footer';
import { DocModal } from './components/DocModal';
import { ApiModal } from './components/ApiModal';
import { DownloadModal } from './components/DownloadModal';

export default function App() {
  const [isDocsOpen, setIsDocsOpen] = useState(false);
  const [isApiOpen, setIsApiOpen] = useState(false);
  const [isDownloadOpen, setIsDownloadOpen] = useState(false);

  return (
    <div className="min-h-screen bg-white text-gray-900 font-sans selection:bg-gray-200 selection:text-gray-900 flex flex-col">
      {/* Top Navigation */}
      <Navbar
        onOpenDocs={() => setIsDocsOpen(true)}
        onOpenApi={() => setIsApiOpen(true)}
        onOpenDownload={() => setIsDownloadOpen(true)}
      />

      {/* Main Page: Exact Wireframe Sequence */}
      <main className="flex-1">
        {/* Hero Section */}
        <HeroSection
          onOpenDownload={() => setIsDownloadOpen(true)}
          onOpenDocs={() => setIsDocsOpen(true)}
        />

        {/* Core Features Section */}
        <CoreFeatures />

        {/* Deploy in 2 Minutes Section */}
        <DeploySection />
      </main>

      {/* Wireframe Footer */}
      <Footer
        onOpenDownload={() => setIsDownloadOpen(true)}
        onOpenDocs={() => setIsDocsOpen(true)}
        onOpenApi={() => setIsApiOpen(true)}
      />

      {/* Functional Modals for Docs, API, and Downloads */}
      <DocModal
        isOpen={isDocsOpen}
        onClose={() => setIsDocsOpen(false)}
      />
      <ApiModal
        isOpen={isApiOpen}
        onClose={() => setIsApiOpen(false)}
      />
      <DownloadModal
        isOpen={isDownloadOpen}
        onClose={() => setIsDownloadOpen(false)}
      />
    </div>
  );
}
