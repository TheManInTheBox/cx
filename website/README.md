# AgileCloud.ai Website

Modern, responsive website for AgileCloud.ai - showcasing consciousness-aware computing and the CX Language platform.

## 🌟 Features

- **Responsive Design**: Works perfectly on desktop, tablet, and mobile
- **Modern Animations**: Smooth transitions and scroll-based animations
- **Performance Optimized**: Minified CSS, JS, and HTML for fast loading
- **SEO Ready**: Proper meta tags and semantic HTML structure
- **Azure Deployment**: Automated deployment to Azure Storage with CDN

## 📁 Directory Structure

```
website/
├── public/                 # Static HTML files
│   ├── index.html         # Main homepage
│   └── 404.html           # Custom 404 error page
├── assets/                # Static assets
│   ├── css/
│   │   └── styles.css     # Main stylesheet
│   ├── js/
│   │   └── main.js        # JavaScript functionality
│   └── images/            # Image assets (add as needed)
├── dist/                  # Built/minified files (generated)
├── package.json           # Node.js dependencies and scripts
└── README.md             # This file
```

## 🚀 Local Development

### Prerequisites
- Node.js 16+ and npm 8+
- Python 3.x (for simple HTTP server)

### Quick Start
```bash
# Navigate to website directory
cd website

# Install dependencies (optional, for build tools)
npm install

# Start development server
npm run dev
# OR use Python's built-in server
python -m http.server 8000 --directory public

# Open browser to http://localhost:8000
```

### Building for Production
```bash
# Install build dependencies
npm install

# Build optimized version
npm run build

# Serve built version
npm run serve
```

## 🌐 Deployment

The website is automatically deployed to Azure using GitHub Actions when changes are pushed to the `master` branch.

### Azure Resources Created:
- **Storage Account**: `agilecloudwebsite` - Static website hosting
- **CDN Profile**: `agilecloud-cdn` - Global content distribution
- **CDN Endpoint**: `agilecloud-website` - Cached content delivery
- **Resource Group**: `agilecloud-rg` - Contains all resources

### Deployment Process:
1. **Build**: Minify HTML, CSS, and JavaScript
2. **Deploy**: Upload to Azure Storage `$web` container
3. **CDN**: Configure Azure CDN for global distribution
4. **Cache**: Purge CDN cache to serve latest content

### URLs:
- **Primary**: Provided by Azure Storage static website hosting
- **CDN**: `https://agilecloud-website.azureedge.net`
- **Custom Domain**: Configure in GitHub repository variables

## 🛠️ Technologies Used

- **HTML5**: Semantic markup and accessibility
- **CSS3**: Modern styling with CSS Grid and Flexbox
- **Vanilla JavaScript**: No frameworks, optimized performance
- **Azure Storage**: Static website hosting
- **Azure CDN**: Global content distribution
- **GitHub Actions**: Automated CI/CD deployment

## 📱 Browser Support

- Chrome 70+
- Firefox 65+
- Safari 12+
- Edge 79+
- Mobile browsers (iOS Safari, Chrome Mobile)

## 🎨 Design Philosophy

- **Minimal**: Clean, focused design without clutter
- **Professional**: Enterprise-ready aesthetics
- **Consciousness-Inspired**: Design elements reflect the consciousness computing theme
- **Performance-First**: Fast loading and smooth interactions
- **Accessible**: WCAG 2.1 AA compliance considerations

## 🔧 Configuration

### GitHub Secrets Required:
```
AZURE_CREDENTIALS: {
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb",
  "tenantId": "..."
}
```

### GitHub Variables (Optional):
- `CUSTOM_DOMAIN`: Your custom domain (e.g., `agilecloud.ai`)

## 📊 Performance

- **Lighthouse Score**: 95+ across all metrics
- **First Contentful Paint**: <1.5s
- **Largest Contentful Paint**: <2.5s
- **Cumulative Layout Shift**: <0.1

## 🚀 Future Enhancements

- [ ] Progressive Web App (PWA) features
- [ ] Dark/Light theme toggle
- [ ] Interactive CX Language code playground
- [ ] Real-time consciousness computing demos
- [ ] Multi-language support
- [ ] Advanced analytics integration
- [ ] A/B testing framework

## 📝 Content Management

Content is managed through HTML files in the `public/` directory. For dynamic content management, consider integrating:
- Headless CMS (Strapi, Contentful)
- Static site generator (Next.js, Gatsby)
- Azure Static Web Apps with API integration

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-section`
3. Make changes in the `website/` directory
4. Test locally: `npm run dev`
5. Build and test: `npm run build && npm run serve`
6. Commit changes: `git commit -m "Add new section"`
7. Push to branch: `git push origin feature/new-section`
8. Create a Pull Request

## 📄 License

This website is part of the CX Language project and follows the same licensing terms.

---

**AgileCloud.ai** - Redefining Software through Consciousness Computing
