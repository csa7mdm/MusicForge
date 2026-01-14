# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.1.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability within MusicForge, please follow these steps:

### Do NOT

- Open a public GitHub issue
- Discuss the vulnerability on public forums
- Share details with others before it's fixed

### Do

1. **Email**: Send details to security@musicforge.dev (if available) or the maintainers directly
2. **Encrypt**: Use GPG if possible for sensitive details
3. **Include**:
   - Type of vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

### Response Timeline

- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 7 days
- **Fix Target**: Within 30 days for critical issues

### After Resolution

- You may be credited in release notes (with your permission)
- A CVE may be assigned if applicable
- Public disclosure coordinated with you

## Security Best Practices for Users

### API Keys

```bash
# Never commit API keys
echo "GROQ_API_KEY=your_key" > .env
echo ".env" >> .gitignore
```

### Docker Security

```yaml
# Run containers as non-root (already configured)
# Limit resources
services:
  api:
    deploy:
      resources:
        limits:
          memory: 2G
```

### Network Security

- Use HTTPS in production
- Configure proper CORS policies
- Use environment-specific configurations

## Known Security Considerations

1. **LLM API Keys**: Stored as environment variables, not in code
2. **Database**: SQLite file should be protected with proper file permissions
3. **gRPC**: Internal communication, not exposed publicly in production
4. **Audio Files**: Validate and sanitize file paths

## Updates

Security patches are released as part of regular updates. Always use the latest version.
